using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Server.Model;
using Server.Model.DTO;

namespace Server.Hub
{
    public class LoadUserChat : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly ApplicationContextDB _db;

        public LoadUserChat(ApplicationContextDB db)
        {
            _db = db;
        }

        public async Task SearchUserChats(UserDataJson userData)
        {
            var userId = userData.Id;

            if (userId == null)
            {
                var currentUser = await _db.Users.FirstAsync(s => s.Email == userData.Email || s.Login == userData.Login);
                userId = currentUser.Id;
            }

            var userChats = await _db.UserChats.Where(s => s.UserId == userId).Include(s => s.Chat).ToListAsync();

            var userChatsDTO = userChats.Select(s => new ChatModel
            {
                Id = s.Chat.Id,
                Name = s.Chat.Name,
                Users = [],
                Messages = []
            }).ToList();

            await Clients.Caller.SendAsync("ReceiveUserChat", userChatsDTO);
        }
        public async Task GetDataChat(int chatId)
        {
            var chat = await _db.Chats.Where(c => c.Id == chatId).Include(c => c.Messages).Include(c => c.Users).ThenInclude(uc => uc.User).FirstOrDefaultAsync();

            if (chat == null)
            {
                await Clients.Caller.SendAsync("ReceiveDataChat", null);
                return;
            }

            var chatDTO = new ChatDTO
            {
                Id = chat.Id,
                Name = chat.Name,
                Users = chat.Users.Select(uc => new UserDTO
                {
                    Id = uc.User!.Id,
                    Name = uc.User.Name,
                    Login = uc.User.Login,
                    Email = uc.User.Email,
                }).ToList(),
                Messages = chat.Messages.Select(m => new MessageDTO
                {
                    Id = m.Id,
                    Content = m.Content,
                    Timestamp = m.Timestamp,
                    ChatId = m.ChatId,
                    UserId = m.UserId,
                }).ToList()
            };

            await Clients.Caller.SendAsync("ReceiveDataChat", chatDTO);
        }
    }
}