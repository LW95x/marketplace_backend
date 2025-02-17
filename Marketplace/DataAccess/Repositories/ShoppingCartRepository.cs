using Marketplace.DataAccess.DbContexts;
using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.DataAccess.Repositories
{
    public class ShoppingCartRepository : IShoppingCartRepository
    {
        private readonly MarketplaceContext _context;

        public ShoppingCartRepository(MarketplaceContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<ShoppingCart> GetShoppingCartByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            return await _context.ShoppingCarts
                .Include(c => c.Items)
                .SingleAsync(c => c.BuyerId == userId);
        }

        public async Task<ShoppingCartItem?> GetSingleShoppingCartItem(string userId, Guid cartItemId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (cartItemId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(cartItemId));
            }

            return await _context.ShoppingCartItems
                .Include(i => i.ShoppingCart)
                .SingleOrDefaultAsync(i => i.Id == cartItemId && i.ShoppingCart.BuyerId == userId);
        }

        public async Task<ShoppingCartItem> AddProductToShoppingCart(ShoppingCartItem shoppingCartItem, string userId)
        {
            var userShoppingCart = await _context.ShoppingCarts
                                         .Include(c => c.Items)
                                         .SingleAsync(c => c.BuyerId == userId);

            userShoppingCart.Items.Add(shoppingCartItem);
            await _context.SaveChangesAsync();
            return shoppingCartItem;
        }

        public async Task<Result> DeleteShoppingCartItemAsync(ShoppingCartItem shoppingCartItem)
        {
            try
            {
                _context.ShoppingCartItems.Remove(shoppingCartItem);
                await _context.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public async Task UpdateCartAsync(ShoppingCart cart)
        {
            if (_context.ChangeTracker.HasChanges())
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ShoppingCartItem> UpdateShoppingCartItemQuantity(ShoppingCartItem shoppingCartItem)
        {
            _context.ShoppingCartItems.Update(shoppingCartItem);
            await _context.SaveChangesAsync();
            return shoppingCartItem;
        }
    }
}
