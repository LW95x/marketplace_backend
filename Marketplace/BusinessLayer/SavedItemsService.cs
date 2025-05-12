using Marketplace.DataAccess.Entities;
using Marketplace.DataAccess.Repositories;
using Marketplace.Helpers;

namespace Marketplace.BusinessLayer
{
    public class SavedItemsService : ISavedItemsService
    {
        private readonly ISavedItemsRepository _savedItemsRepository;
        private readonly ILogger<SavedItemsService> _logger;

        public SavedItemsService(ISavedItemsRepository savedItemsRepository, ILogger<SavedItemsService> logger)
        {
            _savedItemsRepository = savedItemsRepository ?? throw new ArgumentNullException(nameof(savedItemsRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<SavedItem>> FetchSavedItemsByUserId(string userId)
        {
            return await _savedItemsRepository.GetSavedItemsByUserId(userId);
        }

        public async Task<SavedItem?> FetchSingleSavedItem(string userId, Guid productId)
        {
            return await _savedItemsRepository.GetSingleSavedItem(userId, productId);
        }

        public async Task<SavedItem> AddSavedItem(SavedItem savedItem)
        {
            return await _savedItemsRepository.AddProductToSavedItems(savedItem);
        }

        public async Task<Result> RemoveSavedItem(SavedItem savedItem)
        {
            return await _savedItemsRepository.DeleteSavedItem(savedItem);
        }
    }
}
