
using Marketplace.DataAccess.DbContexts;
using Marketplace.DataAccess.Entities;
using Marketplace.DataAccess.Services;
using Marketplace.Helpers;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SQLitePCL;
using System.Reflection.Metadata;

namespace Marketplace.BusinessLayer
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IProductRepository _productRepository;
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly ILogger<ShoppingCartService> _logger;

        public ShoppingCartService(IProductRepository productRepository, IShoppingCartRepository shoppingCartRepository, ILogger<ShoppingCartService> logger)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));   
            _shoppingCartRepository = shoppingCartRepository ?? throw new ArgumentNullException(nameof(shoppingCartRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ShoppingCart> FetchShoppingCartByUserId(string userId)
        {
            return await _shoppingCartRepository.GetShoppingCartByUserId(userId);
        }

        public async Task<ShoppingCartItem?> FetchSingleShoppingCartItem(string userId, Guid cartItemId)
        {
            return await _shoppingCartRepository.GetSingleShoppingCartItem(userId, cartItemId);
        }

        public async Task<ShoppingCartItem> AddShoppingCartItem(ShoppingCartItem shoppingCartItem, string userId)
        {
            try
            {
             shoppingCartItem.TotalPrice = RoundPrice(shoppingCartItem.Price * shoppingCartItem.Quantity);

            var addedItem = await _shoppingCartRepository.AddProductToShoppingCart(shoppingCartItem, userId); 

            var shoppingCart = await _shoppingCartRepository.GetShoppingCartByUserId(userId);
            UpdateTotalPrice(shoppingCart);

            await _shoppingCartRepository.UpdateCartAsync(shoppingCart);

            return addedItem;
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Exception while adding product to shopping cart." + ex.Message);
                throw new InvalidOperationException("Failed to add the product to the Shopping Cart.", ex);
            }
        }

        public async Task<Result> RemoveShoppingCartItem(ShoppingCartItem shoppingCartItem, string userId)
        {
            var deletedItem = await _shoppingCartRepository.DeleteShoppingCartItemAsync(shoppingCartItem);

            if (deletedItem.Succeeded)
            {
                var shoppingCart = await _shoppingCartRepository.GetShoppingCartByUserId(userId);
                UpdateTotalPrice(shoppingCart);
                await _shoppingCartRepository.UpdateCartAsync(shoppingCart);
            }

            return deletedItem;
        }

        public async Task<ShoppingCartItem> UpdateShoppingCartItemQuantity(ShoppingCartItem shoppingCartItem, string userId)
        {
            try
            {
            shoppingCartItem.TotalPrice = RoundPrice(shoppingCartItem.Price * shoppingCartItem.Quantity);

            var updatedItem = await _shoppingCartRepository.UpdateShoppingCartItemQuantity(shoppingCartItem);

            var shoppingCart = await _shoppingCartRepository.GetShoppingCartByUserId(userId);
            UpdateTotalPrice(shoppingCart);
            await _shoppingCartRepository.UpdateCartAsync(shoppingCart);

            return updatedItem;
            }
            catch (Exception ex)
            {
                
                throw new InvalidOperationException("Failed to update the Shopping Cart item's quantity.", ex);
            }
        }

        private decimal RoundPrice(decimal price)
        {
            return Math.Round(price, 2);
        }

        private decimal UpdateTotalPrice(ShoppingCart shoppingCart)
        {
            return shoppingCart.TotalPrice = shoppingCart.Items.Sum(i => i.TotalPrice);
        }
    }
}
