using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;

namespace Marketplace.BusinessLayer
{
    public interface IProductService
    {
        Task<(IEnumerable<Product>, PaginationMetadata)> FetchProductsAsync(string? title, string? category, decimal? minPrice, decimal? maxPrice, int pageNumber, int pageSize);
        Task<Product?> FetchProductByIdAsync(Guid id);
    }
}
