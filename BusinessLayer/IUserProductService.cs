
using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;

namespace Marketplace.BusinessLayer
{
    public interface IUserProductService
    {
        Task<bool> CheckUserExists(string userId);
        Task<Product?> FetchSingleUserProduct(string userId, Guid productId);
        Task<IEnumerable<Product>> FetchUserProductsAsync(string userId);
        Task<Product> CreateProductAsync(Product product);
        Task<Result> RemoveProduct(Product product);
        Task<Product> UpdateProductAsync(Product product);

    }
}
