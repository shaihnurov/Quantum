using Microsoft.AspNetCore.SignalR;

namespace Server.Hub
{
    public class ChatHub : Microsoft.AspNetCore.SignalR.Hub
    {
        public async Task SendMessage(string userName, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", userName, message);
        }
    }
}