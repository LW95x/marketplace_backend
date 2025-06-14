﻿using Marketplace.DataAccess.DbContexts;
using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.DataAccess.Repositories
{
    public class UserProductRepository : IUserProductRepository
    {
        private readonly MarketplaceContext _context;

        public UserProductRepository(MarketplaceContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<bool> UserExistsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            return await _context.Users.AnyAsync(u => u.Id == userId);
        }

        public async Task<IEnumerable<Product>> GetUserProductsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            return await _context.Products
                .Where(p => p.SellerId == userId)
                .Include(p => p.Images)
                .ToListAsync();
        }

        public async Task<Product?> GetSingleUserProductAsync(string userId, Guid productId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (productId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(productId));
            }

            return await _context.Products
                .Where(p => p.SellerId == userId && p.Id == productId)
                .Include(p => p.Images)
                .FirstOrDefaultAsync();
        }

        public async Task<Product> AddProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Result> DeleteProductAsync(Product product)
        {
            try
            {
                var productDependencies = await _context.Products
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.Id == product.Id);

                if (productDependencies == null)
                {
                    return Result.Fail("Product could not be found.");
                }

                var savedItems = await _context.SavedItem
                    .Where(s => s.ProductId == product.Id)
                    .ToListAsync();

                _context.SavedItem.RemoveRange(savedItems);

                _context.ProductImages.RemoveRange(productDependencies.Images);
                _context.Products.Remove(productDependencies);

                await _context.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            var existingProduct = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            if (existingProduct == null)
            {
                return null!;
            }

            if (product.Images != null)
            {
                var newImages = product.Images.ToList();

                existingProduct.Images.Clear();

                foreach (var image in newImages)
                {
                    existingProduct.Images.Add(new ProductImage(image.Url) { ProductId = existingProduct.Id });
                }
            }

            await _context.SaveChangesAsync();
            return existingProduct;
        }
    }
}
