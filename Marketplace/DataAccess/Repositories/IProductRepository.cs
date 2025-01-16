using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;
using Marketplace.Models;

namespace Marketplace.DataAccess.Services
{
    public interface IProductRepository
    {
        Task<(IEnumerable<Product>, PaginationMetadata)> GetProductsAsync(string? title, string? category, decimal? minPrice, decimal? maxPrice, int pageNumber, int pageSize);
        Task<Product?> GetProductByIdAsync(Guid Id);
    }
}

