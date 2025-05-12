using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;

namespace Marketplace.BusinessLayer
{
    public interface ISavedItemsService
    {
        Task<IEnumerable<SavedItem>> FetchSavedItemsByUserId(string userId);
        Task<SavedItem?> FetchSingleSavedItem(string userId, Guid productId);
        Task<SavedItem> AddSavedItem(SavedItem savedItem);
        Task<Result> RemoveSavedItem(SavedItem savedItem);
    }
}
