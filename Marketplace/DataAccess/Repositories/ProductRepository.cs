using Marketplace.DataAccess.DbContexts;
using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace Marketplace.DataAccess.Services
{
    public class ProductRepository : IProductRepository
    {
        private readonly MarketplaceContext _context;

        public ProductRepository(MarketplaceContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); 
        }

        public async Task<(IEnumerable<Product>, PaginationMetadata)> GetProductsAsync(string? title, string? category, decimal? minPrice, decimal? maxPrice, int pageNumber, int pageSize)
        {

            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(title))
            {
                title = title.Trim().ToLower();
                query = query.Where(p => p.Title.ToLower().Contains(title));
            }

            if (!string.IsNullOrEmpty(category))
            {
                category = category.Trim().ToLower();
                query = query.Where(p => p.Category.ToLower().Contains(category));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            var totalItemCount = await query.CountAsync();

            var paginationMetadata = new PaginationMetadata(totalItemCount, pageSize, pageNumber);

            var collection = await query
                .Include(p => p.Images)
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToListAsync();

            return (collection, paginationMetadata);
        }
 
        public async Task<Product?> GetProductByIdAsync(Guid productId)
        {
            return await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(i => i.Id == productId);
        }
    }
}
