using Marketplace.DataAccess.Entities;
using Marketplace.DataAccess.Services;

namespace Marketplace.BusinessLayer
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }
        public async Task<IEnumerable<Product>> FetchProductsAsync()
        {
            return await _productRepository.GetProductsAsync();
        }

        public async Task<Product?> FetchProductByIdAsync(Guid productId)
        {
            return await _productRepository.GetProductByIdAsync(productId);
        }
    }
}
