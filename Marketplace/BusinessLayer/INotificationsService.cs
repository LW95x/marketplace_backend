

using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;

namespace Marketplace.BusinessLayer
{
    public interface INotificationsService
    {
        Task<IEnumerable<Notification>> FetchNotifications(string userId);
        Task<Notification> FetchSingleNotification(string userId, Guid notificationId);
        Task<Notification> AddNotification(Notification notification);
        Task<Result> RemoveNotification(Notification notification);
    }
}
