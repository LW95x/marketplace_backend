

using Marketplace.DataAccess.Repositories;

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
    }
}
