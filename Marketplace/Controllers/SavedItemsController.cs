using AutoMapper;
using Marketplace.BusinessLayer;
using Marketplace.DataAccess.Entities;
using Marketplace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Controllers
{
    [Route("/users/{userId}/saved-items")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    public class SavedItemsController : ControllerBase
    {
        private readonly ISavedItemsService _savedItemsService;
        private readonly IUserProductService _userProductService;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;
        private readonly ILogger<SavedItemsController> _logger;

        public SavedItemsController(ISavedItemsService savedItemsService, IUserProductService userProductService, IProductService productService, IMapper mapper, ILogger<SavedItemsController> logger)
        {
            _savedItemsService = savedItemsService ?? throw new ArgumentNullException(nameof(savedItemsService));
            _userProductService = userProductService ?? throw new ArgumentNullException(nameof(userProductService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        /// <summary>
        /// Get a user's saved items list in it's entirety. 
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<SavedItemForResponseDto>>> GetSavedItemsByUserId(string userId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This User ID does not exist.");
            }

            var userSavedItems = await _savedItemsService.FetchSavedItemsByUserId(userId);

            return Ok(_mapper.Map<IEnumerable<SavedItemForResponseDto>>(userSavedItems));
        }
        /// <summary>
        /// Get a singular saved item belonging to a user.
        /// </summary>
        [HttpGet("{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SavedItemForResponseDto>> GetSingleSavedItem(string userId, Guid productId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This User ID does not exist.");
            }

            var userSingleSavedItem = await _savedItemsService.FetchSingleSavedItem(userId, productId);

            if (userSingleSavedItem == null)
            {
                _logger.LogError($"Saved Item wasn't found for this user.");
                return NotFound("This Saved Item ID does not exist for this user.");
            }

            return Ok(_mapper.Map<SavedItemForResponseDto>(userSingleSavedItem));
        }
        /// <summary>
        /// This method will attempt to add a product to the user's saved item's list (201).
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SavedItemForResponseDto>> AddProductToSavedItems(Guid productId, string userId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This User ID does not exist.");
            }

            var product = await _productService.FetchProductByIdAsync(productId);

            if (product == null)
            {
                _logger.LogError($"Product with ID {productId} wasn't found.");
                return NotFound("Product ID could not be found.");
            }

            var existingProduct = await _savedItemsService.FetchSingleSavedItem(userId, productId);

            if (existingProduct != null)
            {
                _logger.LogError($"This user already has this product in their saved item's list.");
                return BadRequest("Product ID already exists in user's saved item's list.");
            }

            var savedItem = new SavedItem
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                UserId = userId
            };

            var addedSavedItem = await _savedItemsService.AddSavedItem(savedItem);

            if (addedSavedItem == null)
            {
                _logger.LogCritical($"Failed to add the product with ID {productId} to the user's saved item's list");
                return StatusCode(500, "Failed to add the product to the user's saved item's list due to an internal server error.");
            }

            var addedSavedItemResponseDto = _mapper.Map<SavedItemForResponseDto>(addedSavedItem);

            return CreatedAtAction("GetSingleSavedItem", new { userId = userId, productId = productId },
                addedSavedItemResponseDto);
        }

        /// <summary>
        /// Delete a product from a user's saved item's list.
        /// </summary>
        [HttpDelete("{productId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteSavedItem(string userId, Guid productId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This User ID does not exist.");
            }

            var savedItem = await _savedItemsService.FetchSingleSavedItem(userId, productId);

            if (savedItem == null)
            {
                _logger.LogError($"Saved Item with ID {productId} wasn't found.");
                return NotFound("This Saved Item ID does not exist.");
            }

            var result = await _savedItemsService.RemoveSavedItem(savedItem);

            if (result.Succeeded)
            {
                return NoContent();
            }
            else
            {
                _logger.LogCritical($"Failed to remove the Saved Item.");
                return StatusCode(500, result.Error);
            }
        }
    }
}
