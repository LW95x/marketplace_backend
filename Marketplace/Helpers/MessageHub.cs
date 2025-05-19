using Microsoft.AspNetCore.SignalR;

namespace Marketplace.Helpers
{
    public class MessageHub : Hub
    {
        public async Task SendMessage(string senderId, string receiverId, string content)
        {
            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, content);
        }
    }
}
