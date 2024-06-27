
using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;

namespace Marketplace.DataAccess.Services
{
    public interface IUserProductRepository
    {
        Task<bool> UserExistsAsync(string userId);
        Task<IEnumerable<Product>> GetUserProductsAsync(string userId);
        Task<Product?> GetSingleUserProductAsync(string userId, Guid productId);
        Task<Product> AddProductAsync(Product product);
        Task<Result> DeleteProductAsync(Product product);
        Task<Product> UpdateProductAsync(Product product);
    }
}
