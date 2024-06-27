using AutoMapper;
using Marketplace.BusinessLayer;
using Marketplace.DataAccess.Entities;
using Marketplace.DataAccess.Services;
using Marketplace.Helpers;
using Marketplace.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;


namespace Marketplace.Controllers
{
    [Route("/users/{userId}/products")]
    [ApiController]
    public class UserProductsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUserProductService _userProductService;
        private readonly IUserService _userService;

        public UserProductsController(IMapper mapper, IUserProductService userProductService, IUserService userService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userProductService = userProductService ?? throw new ArgumentNullException(nameof(userProductService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [HttpGet] 
        public async Task<ActionResult<IEnumerable<ProductForResponseDto>>> GetUserProducts(string userId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                return NotFound("This User ID does not exist.");
            }

            var userProducts = await _userProductService.FetchUserProductsAync(userId);

            return Ok(_mapper.Map<IEnumerable<ProductForResponseDto>>(userProducts));
        }

        [HttpGet("{productId}", Name = "GetSingleUserProduct")]
        public async Task<ActionResult<ProductForResponseDto>> GetSingleUserProduct(string userId, Guid productId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                return NotFound("This User ID does not exist.");
            }

            var userProduct = await _userProductService.FetchSingleUserProduct(userId, productId);

            if (userProduct == null)
            {
                return NotFound("This Product ID does not exist.");
            }

            return Ok(_mapper.Map<ProductForResponseDto>(userProduct));
        }

        [HttpPost]
        public async Task<ActionResult> CreateProduct(ProductForCreationDto productDto, string userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.FetchUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound("This User ID does not exist.");
            }

            var product = _mapper.Map<Product>(productDto);
            product.SellerId = user.Id;
            product.SellerName = user.UserName;

            var createdProduct = await _userProductService.CreateProductAsync(product);

            if (createdProduct == null)
            {
                return BadRequest("Failed to create product.");
            }

            var productResponseDto = _mapper.Map<ProductForResponseDto>(createdProduct);

            return CreatedAtAction("GetSingleUserProduct", new { userId = createdProduct.SellerId, productId = createdProduct.Id }, productResponseDto);

        }

        [HttpDelete("{productId}")]
        public async Task<ActionResult> DeleteProduct(string userId, Guid productId)
        {
            var product = await _userProductService.FetchSingleUserProduct(userId, productId);

            if (product == null)
            {
                return NotFound("This Product ID does not exist.");
            }

            var result = await _userProductService.RemoveProduct(product);

            if (result.Succeeded)
            {
                return NoContent();
            }
            else
            {
                return BadRequest(result.Error);
            }
        }

        [HttpPatch("{productId}")]
        public async Task<ActionResult> UpdateProduct(string userId, Guid productId, [FromBody] JsonPatchDocument<ProductForUpdateDto> patchDocument)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                return NotFound("This User ID does not exist.");
            }

            var product = await _userProductService.FetchSingleUserProduct(userId, productId);

            if (product == null)
            {
                return NotFound("This Product ID does not exist.");
            }

            var productToPatch = _mapper.Map<ProductForUpdateDto>(product);

            patchDocument.ApplyTo(productToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _mapper.Map(productToPatch, product);

            var updatedProduct = await _userProductService.UpdateProductAsync(product);

            if (updatedProduct == null)
            {
                return BadRequest("Failed to update product.");
            }

            return NoContent();
        }
    }
}
