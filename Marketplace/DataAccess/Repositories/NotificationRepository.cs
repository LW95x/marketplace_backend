using Marketplace.DataAccess.DbContexts;

namespace Marketplace.DataAccess.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly MarketplaceContext _context;

        public NotificationRepository(MarketplaceContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
    }
}
