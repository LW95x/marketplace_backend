using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;

namespace Marketplace.DataAccess.Repositories
{
    public interface IMessageRepository
    {
        Task<IEnumerable<Message>> GetMessagesBetweenUsers(string senderId, string receiverId);
        Task<IEnumerable<Message>> GetAllUserMessages(string userId);
        Task<Result> DeleteMessage(string userId, string messageId);
        Task<Message> SaveMessage(string senderId, string receiverId, string content);
    }
}
