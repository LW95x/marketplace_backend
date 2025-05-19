using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;

namespace Marketplace.BusinessLayer
{
    public interface IMessagesService
    {
        Task<IEnumerable<Message>> FetchMessagesBetweenUsers(string senderId, string receiverId);
        Task<IEnumerable<Message>> FetchAllUserMessages(string userId);
        Task<Result> RemoveMessage(string userId, string messageId);
    }
}
