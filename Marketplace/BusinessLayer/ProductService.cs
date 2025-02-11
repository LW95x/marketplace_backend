using Marketplace.DataAccess.Entities;
using Marketplace.DataAccess.Repositories;
using Marketplace.Helpers;

namespace Marketplace.BusinessLayer
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }
        public async Task<(IEnumerable<Product>, PaginationMetadata)> FetchProductsAsync(string? title, string? category, decimal? minPrice, decimal? maxPrice, int pageNumber, int pageSize)
        {
            return await _productRepository.GetProductsAsync(title, category, minPrice, maxPrice, pageNumber, pageSize);
        }

        public async Task<Product?> FetchProductByIdAsync(Guid productId)
        {
            return await _productRepository.GetProductByIdAsync(productId);
        }
    }
}
