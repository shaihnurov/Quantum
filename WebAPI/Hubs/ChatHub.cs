using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using WebAPI.Models;
using WebAPI.Service;

namespace WebAPI.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly UserConnectionsService _userConnectionsService;
        private readonly ApplicationContextDB _db;

        public ChatHub(UserConnectionsService userConnectionsService, ApplicationContextDB db)
        {
            _userConnectionsService = userConnectionsService;
            _db = db;
        }

        public async Task SendMessage(string message, int chatId)
        {
            var userName = Context.User?.Identity?.Name;
            var timestamp = DateTime.UtcNow;

            await Clients.All.SendAsync("ReceiveMessage", userName, message, timestamp);

            await SaveMessageContext(message, chatId, timestamp);
        }
        private async Task SaveMessageContext(string message, int chatId, DateTime timestamp)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var chatModel = new MessageModel
            {
                Content = message,
                Timestamp = timestamp,
                ChatId = chatId,
                UserId = int.Parse(userId)
            };

            await _db.Messages.AddAsync(chatModel);
            await _db.SaveChangesAsync();
        }

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User?.Identity?.Name;
            Console.WriteLine($"class.ChatHub: Подключение пользователя {userName}");
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.Parse(userId) > 0)
            {
                _userConnectionsService.AddConnection(Context.ConnectionId, int.Parse(userId));
                Console.WriteLine($"ConnectionID ChatHub - {Context.ConnectionId}");
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