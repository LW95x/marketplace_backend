using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;

namespace Marketplace.DataAccess.Repositories
{
    public interface ISavedItemsRepository
    {
        Task<IEnumerable<SavedItem>> GetSavedItemsByUserId(string userId);
        Task<SavedItem?> GetSingleSavedItem(string userId, Guid productId);
        Task<SavedItem> AddProductToSavedItems(SavedItem savedItem);
        Task<Result> DeleteSavedItem(SavedItem savedItem);
    }
}
