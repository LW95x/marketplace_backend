

using Marketplace.Controllers;
using Marketplace.DataAccess.Entities;
using Marketplace.DataAccess.Repositories;
using Marketplace.Helpers;

namespace Marketplace.BusinessLayer
{
    public class NotificationsService : INotificationsService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<NotificationsService> _logger;

        public NotificationsService(INotificationRepository notificationRepository, ILogger<NotificationsService> logger)
        {
            _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Notification>> FetchNotifications(string userId)
        {
            return await _notificationRepository.GetNotifications(userId);
        }

        public async Task<Notification> FetchSingleNotification(string userId, Guid notificationId)
        {
            return await _notificationRepository.GetSingleNotification(userId, notificationId);
        }

        public async Task<Notification> AddNotification(Notification notification)
        {
            return await _notificationRepository.PostNotification(notification);
        }
        
        public async Task<Result> RemoveNotification(Notification notification)
        {
            return await _notificationRepository.DeleteNotification(notification);
        }
    }
}
