using AutoMapper;
using Marketplace.BusinessLayer;
using Marketplace.DataAccess.Entities;
using Marketplace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Controllers
{
    [Route("/users/{userId}/cart")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IUserProductService _userProductService;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;
        private readonly ILogger<ShoppingCartController> _logger;

        public ShoppingCartController(IShoppingCartService shoppingCartService, IUserProductService userProductService, IProductService productService, IMapper mapper, ILogger<ShoppingCartController> logger)
        {
            _shoppingCartService = shoppingCartService ?? throw new ArgumentNullException(nameof(shoppingCartService));
            _userProductService = userProductService ?? throw new ArgumentNullException(nameof(userProductService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        /// <summary>
        /// Get a user's shopping cart in it's entirety. 
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShoppingCartForResponseDto>> GetShoppingCartByUserId(string userId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This User ID does not exist.");
            }

            var userShoppingCart = await _shoppingCartService.FetchShoppingCartByUserId(userId);

            return Ok(_mapper.Map<ShoppingCartForResponseDto>(userShoppingCart));
        }

        /// <summary>
        /// Get a single shopping cart item from a user's shopping cart.
        /// </summary>
        [HttpGet("{cartItemId}", Name = "GetSingleShoppingCartItem")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShoppingCartItemForResponseDto>> GetSingleShoppingCartItem(string userId, Guid cartItemId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This User ID does not exist.");
            }

            var userShoppingCartItem = await _shoppingCartService.FetchSingleShoppingCartItem(userId, cartItemId);

            if (userShoppingCartItem == null)
            {
                _logger.LogError($"Shopping Cart Item with ID {cartItemId} wasn't found.");
                return NotFound("This Shopping Cart Item ID does not exist.");
            }

            return Ok(_mapper.Map<ShoppingCartItemForResponseDto>(userShoppingCartItem));
        }

        /// <summary>
        /// This method will attempt to add a product to the user's shopping cart (201), or update the quantity of an already existing product in the user's shopping cart (204).
        /// </summary>

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ShoppingCartItemForResponseDto>> AddProductToShoppingCart(ShoppingCartItemForCreationDto shoppingCartItemDto, string userId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This User ID does not exist.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError($"Validation has failed for POST Shopping Cart Item request.");
                return BadRequest(ModelState);
            }

            var userShoppingCart = await _shoppingCartService.FetchShoppingCartByUserId(userId);

            if (userShoppingCart == null)
            {
                _logger.LogError($"Shopping Cart attached to User ID {userId} wasn't found.");
                return NotFound("Shopping Cart could not be found.");
            }

            var product = await _productService.FetchProductByIdAsync(shoppingCartItemDto.ProductId);

            if (product == null)
            {
                _logger.LogError($"Product with ID {shoppingCartItemDto.ProductId} wasn't found.");
                return NotFound("Product ID could not be found.");
            }

            if (shoppingCartItemDto.Quantity > product.Quantity)
            {
                _logger.LogError($"User attempted to add product quantity to Shopping Cart in excess of the remaining stock.");
                return BadRequest("This quantity exceeds the maximum available stock.");
            }
            
            var existingCartItem = userShoppingCart.Items.FirstOrDefault(item => item.ProductId == shoppingCartItemDto.ProductId);

            if (existingCartItem != null)
            {
                _logger.LogInformation($"The product added already existed in the User's Shopping Cart - quantity in Shopping Cart has been updated to reflect this addition.");

                return await UpdateShoppingCartItemQuantity(userId, existingCartItem.Id, shoppingCartItemDto.Quantity, true);
            }


            var shoppingCartItem = _mapper.Map<ShoppingCartItem>(shoppingCartItemDto);
            shoppingCartItem.ShoppingCartId = userShoppingCart.Id;
            shoppingCartItem.Price = product.Price;

            var addedCartItem = await _shoppingCartService.AddShoppingCartItem(shoppingCartItem, userId);

            if (addedCartItem == null)
            {
                _logger.LogCritical($"Failed to add the product with ID {shoppingCartItemDto.ProductId} to the shopping cart.");
                return StatusCode(500, "Failed to add the product to the shopping cart due to an internal server error.");
            }

            var addedCartItemResponseDto = _mapper.Map<ShoppingCartItemForResponseDto>(addedCartItem);

            return CreatedAtAction("GetSingleShoppingCartItem", new { userId = userId, cartItemId = addedCartItem.Id }, addedCartItemResponseDto);
        }
        /// <summary>
        /// Delete an item from a user's shopping cart.
        /// </summary>
        [HttpDelete("{cartItemId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteShoppingCartItem(string userId, Guid cartItemId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This User ID does not exist.");
            }

            var shoppingCartItem = await _shoppingCartService.FetchSingleShoppingCartItem(userId, cartItemId);

            if (shoppingCartItem == null)
            {
                _logger.LogError($"Shopping Cart Item with ID {cartItemId} wasn't found.");
                return NotFound("This Shopping Cart Item ID does not exist.");
            }

            var result = await _shoppingCartService.RemoveShoppingCartItem(shoppingCartItem, userId);

            if (result.Succeeded)
            {
                return NoContent();
            }
            else
            {
                _logger.LogCritical($"Failed to remove the Shopping Cart Item.");
                return StatusCode(500, result.Error);
            }
        }
        /// <summary>
        /// Update the quantity of an item in a user's shopping cart.
        /// </summary>
        [HttpPatch("{cartItemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateShoppingCartItemQuantity(string userId, Guid cartItemId, int newQuantity, bool addQuantity = false)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("User ID could not be found.");
            }

            var shoppingCartItem = await _shoppingCartService.FetchSingleShoppingCartItem(userId, cartItemId);

            if (shoppingCartItem == null)
            {
                _logger.LogError($"Shopping Cart Item with ID {cartItemId} wasn't found.");
                return NotFound("This Shopping Cart Item ID does not exist.");
            }

            var product = await _productService.FetchProductByIdAsync(shoppingCartItem.ProductId);

            if (product == null)
            {
                _logger.LogError($"Product with ID {shoppingCartItem.ProductId} wasn't found.");
                return NotFound("This Product ID could not be found.");
            }

            if (newQuantity > product.Quantity)
            {
                _logger.LogError($"User attempted to update product quantity to Shopping Cart in excess of the remaining stock.");
                return BadRequest("This quantity exceeds the maximum available stock.");
            }

            if (addQuantity)
            {
                if (shoppingCartItem.Quantity + newQuantity > product.Quantity)
                {
                    _logger.LogError($"User attempted to update product quantity to Shopping Cart in excess of the remaining stock.");
                    return BadRequest("This added quantity exceeds the maximum available stock.");
                }
                shoppingCartItem.Quantity += newQuantity;
            }
            else
            {
                shoppingCartItem.Quantity = newQuantity;
            }
            
            var result = await _shoppingCartService.UpdateShoppingCartItemQuantity(shoppingCartItem, userId);

            if (result == null)
            {
                _logger.LogCritical($"Failed to update the shopping cart item's quantity.");
                return StatusCode(500, "Failed to update shopping cart item due to an internal server error.");
            }

            return NoContent();
        }
    }
}
