using AutoMapper;
using Marketplace.BusinessLayer;
using Marketplace.DataAccess.Entities;
using Marketplace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Controllers
{
    [Route("/users/{userId}/orders")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IUserProductService _userProductService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IMapper _mapper;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, IUserProductService userProductService, IShoppingCartService shoppingCartService, IMapper mapper, ILogger<OrdersController> logger) 
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _userProductService = userProductService ?? throw new ArgumentNullException(nameof(userProductService));
            _shoppingCartService = shoppingCartService ?? throw new ArgumentNullException(nameof(shoppingCartService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        /// <summary>
        /// Get all orders listed by a specific user.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<OrderForResponseDto>>> GetOrders(string userId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This User ID does not exist.");
            }

            var orders = await _orderService.FetchOrdersAsync(userId);

            return Ok(_mapper.Map<IEnumerable<OrderForResponseDto>>(orders));
        }
        /// <summary>
        /// Get a specific order listed by a specific user.
        /// </summary>
        [HttpGet("{orderId}", Name = "GetOrderById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderForResponseDto>> GetOrderById(string userId, Guid orderId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This User ID does not exist.");
            }

            var order = await _orderService.FetchOrderByIdAsync(userId, orderId);

            if (order == null)
            {
                _logger.LogError($"Order with ID {orderId} wasn't found.");
                return NotFound("This Order ID does not exist.");
            }

            return Ok(_mapper.Map<OrderForResponseDto>(order));
        }
        /// <summary>
        /// Create a new order, by converting a user's shopping cart to an order.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderForResponseDto>> CreateOrder(OrderForCreationDto orderDto, string userId)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Validation has failed for POST Order request.");
                return BadRequest(ModelState);
            }

            if (!await _userProductService.CheckUserExists(userId))
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This User ID does not exist.");
            }

            var cart = await _shoppingCartService.FetchShoppingCartByUserId(userId);

            if (cart == null || !cart.Items.Any())
            {
                _logger.LogError($"The shopping cart is empty and cannot be converted to an order.");
                return BadRequest("Empty shopping cart.");
            }
            
            var order = _mapper.Map<Order>(cart);
            order.Address = orderDto.Address;
            order.StripePaymentId = orderDto.StripePaymentId;

            var createdOrder = await _orderService.CreateOrderAsync(order);

            if (createdOrder == null)
            {
                _logger.LogCritical($"Failed to create the order.");
                return StatusCode(500, "Failed to create the order due to an internal server error.");
            }

            var orderResponseDto = _mapper.Map<OrderForResponseDto>(createdOrder);

            return CreatedAtAction("GetOrderById", new { userId = createdOrder.BuyerId, orderId = createdOrder.Id }, orderResponseDto);
        }
        /// <summary>
        /// Delete an existing order.
        /// </summary>
        [HttpDelete("{orderId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteOrder(string userId, Guid orderId)
        {
            var order = await _orderService.FetchOrderByIdAsync(userId, orderId);

            if (order == null)
            {
                _logger.LogError($"Order with ID {orderId} wasn't found.");
                return NotFound("This Order ID does not exist");
            }

            var result = await _orderService.RemoveOrder(order);

            if (result.Succeeded)
            {
                return NoContent();
            }
            else
            {
                _logger.LogCritical($"Failed to delete the order.");
                return StatusCode(500, result.Error);
            }
        }
        /// <summary>
        /// Update an existing order's OrderStatus or Delivery Address.
        /// </summary>
        [HttpPatch("{orderId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateOrder(string userId, Guid orderId, [FromBody] JsonPatchDocument<OrderForUpdateDto> patchDocument)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This User ID does not exist.");
            }

            var order = await _orderService.FetchOrderByIdAsync(userId, orderId);

            if (order == null)
            {
                _logger.LogError($"Order with ID {orderId} wasn't found.");
                return NotFound("This Order ID does not exist.");
            }

            var orderToPatch = _mapper.Map<OrderForUpdateDto>(order);

            patchDocument.ApplyTo(orderToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                _logger.LogError($"Validation has failed for updated order with ID {orderId}");
                return BadRequest(ModelState);
            }

            _mapper.Map(orderToPatch, order);

            var updatedOrder = await _orderService.UpdateOrderAsync(order);

            if (updatedOrder == null)
            {
                _logger.LogCritical($"Failed to update the order.");
                return StatusCode(500, "Failed to update the order due to an internal server error.");
            }

            return NoContent();
        }
        /// <summary>
        /// Get all sold items for a specific user.
        /// </summary>
        [HttpGet("sold-items")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<SoldItemForResponseDto>>> GetSoldItems(string userId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This User ID does not exist.");
            }

            var soldItems = await _orderService.FetchSoldItems(userId);

            return Ok(_mapper.Map<IEnumerable<SoldItemForResponseDto>>(soldItems));
        }
    }
}
