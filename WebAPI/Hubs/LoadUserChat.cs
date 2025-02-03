using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;
using WebAPI.Models;
using WebAPI.Models.DTO;
using WebAPI.Service;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WebAPI.Hubs
{
    [Authorize]
    public class LoadUserChat : Hub
    {
        private readonly ApplicationContextDB _db;
        private readonly UserConnectionsService _userConnectionsService;

        public LoadUserChat(ApplicationContextDB db, UserConnectionsService userConnectionsService)
        {
            _db = db;
            _userConnectionsService = userConnectionsService;
        }

        public async Task SearchUserChats()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
            var userId = userIdClaim!.Value;

            var userChats = await _db.UserChats.Where(s => s.UserId == int.Parse(userId)).Include(s => s.Chat).ToListAsync();

            var userChatsDTO = userChats.Select(s => new ChatModel
            {
                Id = s.Chat!.Id,
                Name = s.Chat.Name,
                Users = [],
                Messages = []
            }).ToList();

            await Clients.Client(Context.ConnectionId).SendAsync("ReceiveUserChat", userChatsDTO);
        }
        public async Task UpdateChatList(int userId, int chatId)
        {
            var userChat = await _db.UserChats.Where(s => s.UserId == userId && s.ChatId == chatId).Include(s => s.Chat).ToListAsync();

            if (userChat == null)
            {
                Log.Error($"Чат с ID {chatId} не найден для пользователя с ID {userId}");
                await Clients.Client(Context.ConnectionId).SendAsync("ReceiveUserChat", null);
                return;
            }

            var userChatsDTO = userChat.Select(s => new ChatModel
            {
                Id = s.Chat!.Id,
                Name = s.Chat.Name,
                Users = [],
                Messages = []
            }).ToList();

            await Clients.Client(Context.ConnectionId).SendAsync("ReceiveUserChat", userChatsDTO);
        }
        public async Task GetDataChat(int chatId)
        {
            try
            {
                var chat = await _db.Chats
                    .Where(c => c.Id == chatId)
                    .AsSplitQuery()
                    .Include(c => c.Messages)
                    .Include(c => c.Users)
                        .ThenInclude(uc => uc.User)
                    .FirstOrDefaultAsync();

                if (chat == null)
                {
                    await Clients.Caller.SendAsync("ReceiveDataChat", null);
                    return;
                }

                var chatDTO = await _db.Chats
                    .Where(c => c.Id == chatId)
                    .Select(c => new ChatDTO
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Users = c.Users.Select(uc => new UserDTO
                        {
                            Id = uc.User!.Id,
                            Name = uc.User.Name,
                            Login = uc.User.Login,
                            Email = uc.User.Email,
                        }).ToList(),
                        Messages = c.Messages.Select(m => new MessageDTO
                        {
                            Id = m.Id,
                            Content = m.Content,
                            Timestamp = m.Timestamp,
                            ChatId = m.ChatId,
                            UserId = m.UserId,
                        }).ToList()
                    })
                    .AsSplitQuery()
                    .FirstOrDefaultAsync();

                if (chatDTO == null)
                {
                    Log.Error($"Chat with Id {chatId} not found");
                    await Clients.Client(Context.ConnectionId).SendAsync("ReceiveDataChat", null);
                }
                else
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("ReceiveDataChat", chatDTO);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetDataChat");
                await Clients.Client(Context.ConnectionId).SendAsync("ReceiveDataChat", null);
            }
        }
        public async Task LeaveChat(int chatId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userChat = await _db.UserChats.FirstOrDefaultAsync(uc => uc.UserId == int.Parse(userId) && uc.ChatId == chatId);

            if (userChat != null)
            {
                _db.UserChats.Remove(userChat);
                await _db.SaveChangesAsync();

                await Clients.Client(Context.ConnectionId).SendAsync("SuccReceive", "You have left the chat room");
            }
            else
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ErrorReceive", "You are not a member of this chat");
            }
        }

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User?.Identity?.Name;
            Console.WriteLine($"class.LoadUserChar: Подключение пользователя {userName}");
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.Parse(userId) > 0)
            {
                // Сохраняем подключение
                _userConnectionsService.AddConnection(Context.ConnectionId, int.Parse(userId));
                Console.WriteLine($"ConnectionID LoadUserChat - {Context.ConnectionId}");
            }
            else
                Context.Abort();

            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _userConnectionsService.RemoveConnection(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}