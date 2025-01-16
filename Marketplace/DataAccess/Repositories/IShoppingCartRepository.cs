using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;

namespace Marketplace.DataAccess.Services
{
    public interface IShoppingCartRepository
    {
        Task<ShoppingCart> GetShoppingCartByUserId(string userId);
        Task<ShoppingCartItem> AddProductToShoppingCart(ShoppingCartItem shoppingCartItem, string userId);
        Task<ShoppingCartItem?> GetSingleShoppingCartItem(string userId, Guid cartItemId);
        Task UpdateCartAsync(ShoppingCart cart);
        Task<Result> DeleteShoppingCartItemAsync(ShoppingCartItem shoppingCartItem);
        Task<ShoppingCartItem> UpdateShoppingCartItemQuantity(ShoppingCartItem shoppingCartItem);
    }
}
