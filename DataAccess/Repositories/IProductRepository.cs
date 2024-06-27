using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;
using Marketplace.Models;

namespace Marketplace.DataAccess.Services
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetProductsAsync();
        Task<Product?> GetProductByIdAsync(Guid Id);
    }
}

