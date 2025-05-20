using Marketplace.BusinessLayer;
using Marketplace.DataAccess.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace Marketplace.Helpers
{
    public class MessageHub : Hub
    {
        private readonly IMessageRepository _messageRepository;

        public MessageHub(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        }
        public async Task SendMessage(string senderId, string receiverId, string content)
        {
            await _messageRepository.SaveMessage(senderId, receiverId, content);

            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, content);
        }
    }
}
