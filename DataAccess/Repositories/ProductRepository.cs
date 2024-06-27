using Marketplace.DataAccess.DbContexts;
using Marketplace.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.DataAccess.Services
{
    public class ProductRepository : IProductRepository
    {
        private readonly MarketplaceContext _context;

        public ProductRepository(MarketplaceContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); 
        }

        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Images)
                .ToListAsync();
        }
 
        public async Task<Product?> GetProductByIdAsync(Guid productId)
        {
            return await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(i => i.Id == productId);
        }
    }
}
