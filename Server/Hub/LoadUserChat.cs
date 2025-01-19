using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Server.Model;

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
    }
}