using Marketplace.DataAccess.DbContexts;
using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.DataAccess.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly MarketplaceContext _context;

        public NotificationRepository(MarketplaceContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Notification>> GetNotifications(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            return await _context.Notification
                .Where(n => n.UserId == userId)
                .ToListAsync();
        }

        public async Task<Notification?> GetSingleNotification(string userId, Guid notificationId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (notificationId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(notificationId));
            }

            return await _context.Notification
                .Where(n => n.UserId == userId && n.Id == notificationId)
                .FirstOrDefaultAsync();
        }
        
        public async Task<Notification> PostNotification(Notification notification)
        {
            _context.Notification.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }
        public async Task<Result> DeleteNotification(Notification notification)
        {
            try
            {
                _context.Notification.Remove(notification);
                await _context.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }
    }
}
