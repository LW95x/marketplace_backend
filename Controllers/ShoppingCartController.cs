using AutoMapper;
using Marketplace.BusinessLayer;
using Marketplace.DataAccess.Entities;
using Marketplace.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Controllers
{
    [Route("/users/{userId}/cart")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IUserProductService _userProductService;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public ShoppingCartController(IShoppingCartService shoppingCartService, IUserProductService userProductService, IProductService productService, IMapper mapper)
        {
            _shoppingCartService = shoppingCartService ?? throw new ArgumentNullException(nameof(shoppingCartService));
            _userProductService = userProductService ?? throw new ArgumentNullException(nameof(userProductService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<ShoppingCartForResponseDto>> GetShoppingCartByUserId(string userId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                return NotFound("This User ID does not exist.");
            }

            var userShoppingCart = await _shoppingCartService.FetchShoppingCartByUserId(userId);

            return Ok(_mapper.Map<ShoppingCartForResponseDto>(userShoppingCart));
        }

        [HttpGet("{cartItemId}", Name = "GetSingleShoppingCartItem")]
        public async Task<ActionResult<ShoppingCartItemForResponseDto>> GetSingleShoppingCartItem(string userId, Guid cartItemId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                return NotFound("This User ID does not exist.");
            }

            var userShoppingCartItem = await _shoppingCartService.FetchSingleShoppingCartItem(userId, cartItemId);

            return Ok(_mapper.Map<ShoppingCartItemForResponseDto>(userShoppingCartItem));
        }


        [HttpPost]
        public async Task<ActionResult> AddProductToShoppingCart(ShoppingCartItemForCreationDto shoppingCartItemDto, string userId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                return NotFound("This User ID does not exist.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userShoppingCart = await _shoppingCartService.FetchShoppingCartByUserId(userId);

            if (userShoppingCart == null)
            {
                return NotFound("Shopping Cart could not be found.");
            }

            var product = await _productService.FetchProductByIdAsync(shoppingCartItemDto.ProductId);

            if (product == null)
            {
                return NotFound("Product ID could not be found.");
            }

            if (shoppingCartItemDto.Quantity > product.Quantity)
            {
                return BadRequest("This quantity exceeds the maximum available stock.");
            }
            
            var existingCartItem = userShoppingCart.Items.FirstOrDefault(item => item.ProductId == shoppingCartItemDto.ProductId);

            if (existingCartItem != null)
            {
                return await UpdateShoppingCartItemQuantity(userId, existingCartItem.Id, shoppingCartItemDto.Quantity, true);
            }
            

            var shoppingCartItem = _mapper.Map<ShoppingCartItem>(shoppingCartItemDto);
            shoppingCartItem.ShoppingCartId = userShoppingCart.Id;
            shoppingCartItem.Price = product.Price;

            var addedCartItem = await _shoppingCartService.AddShoppingCartItem(shoppingCartItem, userId);

            if (addedCartItem == null)
            {
                throw new InvalidOperationException("Failed to add the item to the shopping cart.");
            }

            var addedCartItemResponseDto = _mapper.Map<ShoppingCartItemForResponseDto>(addedCartItem);

            return CreatedAtAction("GetSingleShoppingCartItem", new { userId = userId, cartItemId = addedCartItem.Id }, addedCartItemResponseDto);
        }

        [HttpDelete("{cartItemId}")]
        public async Task<ActionResult> DeleteShoppingCartItem(string userId, Guid cartItemId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                return NotFound("This User ID does not exist.");
            }

            var shoppingCartItem = await _shoppingCartService.FetchSingleShoppingCartItem(userId, cartItemId);

            if (shoppingCartItem == null)
            {
                return NotFound("This Shopping Cart Item ID does not exist.");
            }

            var result = await _shoppingCartService.RemoveShoppingCartItem(shoppingCartItem, userId);

            if (result.Succeeded)
            {
                return NoContent();
            }
            else
            {
                return BadRequest(result.Error);
            }
        }

        [HttpPatch("{cartItemId}")]
        public async Task<ActionResult> UpdateShoppingCartItemQuantity(string userId, Guid cartItemId, int newQuantity, bool addQuantity = false)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                return NotFound("User ID could not be found.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var shoppingCartItem = await _shoppingCartService.FetchSingleShoppingCartItem(userId, cartItemId);

            if (shoppingCartItem == null)
            {
                return NotFound("This Shopping Cart Item ID does not exist.");
            }

            var product = await _productService.FetchProductByIdAsync(shoppingCartItem.ProductId);

            if (product == null)
            {
                return NotFound("This Product ID could not be found.");
            }

            if (newQuantity > product.Quantity)
            {
                return BadRequest("This quantity exceeds the maximum available stock.");
            }

            if (addQuantity)
            {
                if (shoppingCartItem.Quantity + newQuantity > product.Quantity)
                {
                    return BadRequest("This added quantity exceeds the maximum available stock.");
                }
                shoppingCartItem.Quantity += newQuantity;
            }
            else
            {
                shoppingCartItem.Quantity = newQuantity;
            }
            
            var result = await _shoppingCartService.UpdateShoppingCartItemQuantity(shoppingCartItem, userId);

            return Ok(_mapper.Map<ShoppingCartItemForResponseDto>(result));
        }
    }
}
