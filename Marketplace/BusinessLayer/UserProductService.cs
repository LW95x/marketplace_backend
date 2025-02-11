using Marketplace.DataAccess.Entities;
using Marketplace.DataAccess.Repositories;
using Marketplace.Helpers;

namespace Marketplace.BusinessLayer
{
    public class UserProductService : IUserProductService
    {
        private readonly IUserProductRepository _userProductRepository;

        public UserProductService(IUserProductRepository userProductRepository)
        {
            _userProductRepository = userProductRepository ?? throw new ArgumentNullException(nameof(userProductRepository));
        }

        public async Task<bool> CheckUserExists(string userId)
        {
            return await _userProductRepository.UserExistsAsync(userId);
        }

        public async Task<IEnumerable<Product>> FetchUserProductsAsync(string userId)
        {
            return await _userProductRepository.GetUserProductsAsync(userId);
        }
        public async Task<Product?> FetchSingleUserProduct(string userId, Guid productId)
        {
            return await _userProductRepository.GetSingleUserProductAsync(userId, productId);
        }
        public async Task<Product> CreateProductAsync(Product product)
        {
            product.Price = RoundPrice(product.Price);

            return await _userProductRepository.AddProductAsync(product);
        }

        public async Task<Result> RemoveProduct(Product product)
        {
            return await _userProductRepository.DeleteProductAsync(product);
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            product.Price = RoundPrice(product.Price);

            return await _userProductRepository.UpdateProductAsync(product);
        }

        private decimal RoundPrice(decimal price)
        {
            return Math.Round(price, 2);
        }
    }
}
