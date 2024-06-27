using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;

namespace Marketplace.BusinessLayer
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> FetchProductsAsync();
        Task<Product?> FetchProductByIdAsync(Guid id);
    }
}
