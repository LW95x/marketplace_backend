using AutoMapper;
using Marketplace.BusinessLayer;
using Marketplace.DataAccess.Entities;
using Marketplace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;


namespace Marketplace.Controllers
{
    [Route("/users/{userId}/products")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    public class UserProductsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUserProductService _userProductService;
        private readonly IUserService _userService;
        private readonly ILogger<UserProductsController> _logger;

        public UserProductsController(IMapper mapper, IUserProductService userProductService, IUserService userService, ILogger<UserProductsController> logger)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userProductService = userProductService ?? throw new ArgumentNullException(nameof(userProductService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        /// <summary>
        /// Get all products listed by a specific user.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ProductForResponseDto>>> GetUserProducts(string userId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This User ID does not exist.");
            }

            var userProducts = await _userProductService.FetchUserProductsAsync(userId);

            return Ok(_mapper.Map<IEnumerable<ProductForResponseDto>>(userProducts));
        }
        /// <summary>
        /// Get a specific product listed by a specific user.
        /// </summary>
        [HttpGet("{productId}", Name = "GetSingleUserProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductForResponseDto>> GetSingleUserProduct(string userId, Guid productId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This User ID does not exist.");
            }

            var userProduct = await _userProductService.FetchSingleUserProduct(userId, productId);

            if (userProduct == null)
            {
                _logger.LogError($"Product with ID {productId} wasn't found.");
                return NotFound("This Product ID does not exist.");
            }

            return Ok(_mapper.Map<ProductForResponseDto>(userProduct));
        }
        /// <summary>
        /// Add a new product.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CreateProduct(ProductForCreationDto productDto, string userId)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Validation has failed for POST Product request.");
                return BadRequest(ModelState);
            }

            var user = await _userService.FetchUserByIdAsync(userId);

            if (user == null)
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This User ID does not exist.");
            }

            var product = _mapper.Map<Product>(productDto);
            product.SellerId = user.Id;
            product.SellerName = user.UserName;

            var createdProduct = await _userProductService.CreateProductAsync(product);

            if (createdProduct == null)
            {
                _logger.LogCritical($"Failed to create the product.");
                return StatusCode(500, "Failed to create product due to an internal server error.");
            }

            var productResponseDto = _mapper.Map<ProductForResponseDto>(createdProduct);

            return CreatedAtAction("GetSingleUserProduct", new { userId = createdProduct.SellerId, productId = createdProduct.Id }, productResponseDto);

        }
        /// <summary>
        /// Delete an existing product.
        /// </summary>
        [HttpDelete("{productId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteProduct(string userId, Guid productId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This User ID does not exist.");
            }

            var product = await _userProductService.FetchSingleUserProduct(userId, productId);

            if (product == null)
            {
                _logger.LogError($"Product with ID {productId} wasn't found.");
                return NotFound("This Product ID does not exist.");
            }

            var result = await _userProductService.RemoveProduct(product);

            if (result.Succeeded)
            {
                return NoContent();
            }
            else
            {
                _logger.LogCritical($"Failed to delete the product.");
                return StatusCode(500, result.Error);
            }
        }
        /// <summary>
        /// Update an existing product.
        /// </summary>
        [HttpPatch("{productId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateProduct(string userId, Guid productId, [FromBody] JsonPatchDocument<ProductForUpdateDto> patchDocument)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This User ID does not exist.");
            }

            var product = await _userProductService.FetchSingleUserProduct(userId, productId);

            if (product == null)
            {
                _logger.LogError($"Product with ID {productId} wasn't found.");
                return NotFound("This Product ID does not exist.");
            }

            var productToPatch = _mapper.Map<ProductForUpdateDto>(product);

            patchDocument.ApplyTo(productToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                _logger.LogError($"Validation has failed for PATCH Product request.");
                return BadRequest(ModelState);
            }

            _mapper.Map(productToPatch, product);

            if (productToPatch.ImageUrls != null && patchDocument.Operations.Any(op => op.path.Equals("/imageUrls", StringComparison.OrdinalIgnoreCase)))
            {
                product.Images = productToPatch.ImageUrls
                    .Select(url => new ProductImage(url) { ProductId = product.Id })
                    .ToList();
            }

            var updatedProduct = await _userProductService.UpdateProductAsync(product);

            if (updatedProduct == null)
            {
                _logger.LogCritical($"Failed to update the order.");
                return StatusCode(500, "Failed to update product due to an internal server error.");
            }

            return NoContent();
        }
    }
}
