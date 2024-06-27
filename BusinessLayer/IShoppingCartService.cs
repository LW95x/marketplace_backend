using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;

namespace Marketplace.BusinessLayer
{
    public interface IShoppingCartService
    {
        Task<ShoppingCart> FetchShoppingCartByUserId(string userId);
        Task<ShoppingCartItem?> FetchSingleShoppingCartItem(string userId, Guid cartItemId);
        Task<ShoppingCartItem> AddShoppingCartItem(ShoppingCartItem shoppingCartItem, string userId);
        Task<Result> RemoveShoppingCartItem(ShoppingCartItem shoppingCartItem, string userId);
        Task<ShoppingCartItem> UpdateShoppingCartItemQuantity(ShoppingCartItem shoppingCartItem, string userId);
    }
}
