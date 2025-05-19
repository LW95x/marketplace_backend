using Marketplace.DataAccess.Entities;
using Marketplace.DataAccess.Repositories;
using Marketplace.Helpers;

namespace Marketplace.BusinessLayer
{
    public class MessagesService : IMessagesService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly ILogger<MessagesService> _logger;

        public MessagesService(IMessageRepository messageRepository, ILogger<MessagesService> logger)
        {
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Message>> FetchMessagesBetweenUsers(string senderId, string receiverId)
        {
            return await _messageRepository.GetMessagesBetweenUsers(senderId, receiverId);
        }

        public async Task<IEnumerable<Message>> FetchAllUserMessages(string userId)
        {
            return await _messageRepository.GetAllUserMessages(userId);
        }

        public async Task<Result> RemoveMessage(string userId, string messageId)
        {
            return await _messageRepository.DeleteMessage(userId, messageId);
        }
    }
}
