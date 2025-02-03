using Microsoft.AspNetCore.SignalR;
using Server.Model;

namespace Server.Hub
{
    public class ChatHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly ApplicationContextDB _db;

        public ChatHub(ApplicationContextDB db)
        {
            _db = db;
        }

        public async Task SendMessage(string userName, string message, int chatId, int userId)
        {
            await Clients.All.SendAsync("ReceiveMessage", userName, message);

            await SaveMessageContext(message, chatId, userId);
        }
        private async Task SaveMessageContext(string message, int chatId, int userId)
        {
            var chatModel = new MessageModel
            {
                Content = message,
                Timestamp = DateTime.UtcNow,
                ChatId = chatId,
                UserId = userId
            };

            await _db.Messages.AddAsync(chatModel);
            await _db.SaveChangesAsync();
        }
    }
}