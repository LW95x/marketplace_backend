using Marketplace.DataAccess.DbContexts;
using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.DataAccess.Repositories
{
    public class SavedItemsRepository : ISavedItemsRepository
    {
        private readonly MarketplaceContext _context;

        public SavedItemsRepository(MarketplaceContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<IEnumerable<SavedItem>> GetSavedItemsByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            return await _context.SavedItem
                .Where(s => s.UserId == userId)
                .Include(s => s.Product)
                .ThenInclude(p => p.Images)
                .ToListAsync();
        }

        public async Task<SavedItem?> GetSingleSavedItem(string userId, Guid productId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (productId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(productId));
            }

            return await _context.SavedItem
                .Where(s => s.UserId == userId && s.ProductId == productId)
                .Include(s => s.Product)
                .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync();
        }

        public async Task<SavedItem> AddProductToSavedItems(SavedItem savedItem)
        {
            _context.SavedItem.Add(savedItem);
            await _context.SaveChangesAsync();
            return savedItem;
        }

        public async Task<Result> DeleteSavedItem(SavedItem savedItem) 
        {
            try
            {
                _context.SavedItem.Remove(savedItem);
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
