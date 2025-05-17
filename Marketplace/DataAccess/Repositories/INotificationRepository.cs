using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;

namespace Marketplace.DataAccess.Repositories
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetNotifications(string userId);
        Task<Notification?> GetSingleNotification(string userId, Guid notificationId);
        Task<Notification> PostNotification(Notification notification);
        Task<Result> DeleteNotification(Notification notification);
    }
}
