using AutoMapper;
using Marketplace.BusinessLayer;
using Marketplace.DataAccess.Entities;
using Marketplace.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Marketplace.Controllers
{
    [Route("/users/{userId}/orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IUserProductService _userProductService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IMapper _mapper;

        public OrdersController(IOrderService orderService, IUserProductService userProductService, IShoppingCartService shoppingCartService, IMapper mapper) 
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _userProductService = userProductService ?? throw new ArgumentNullException(nameof(userProductService));
            _shoppingCartService = shoppingCartService ?? throw new ArgumentNullException(nameof(shoppingCartService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderForResponseDto>>> GetOrders(string userId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                return NotFound("This User ID does not exist.");
            }

            var orders = await _orderService.FetchOrdersAsync(userId);

            return Ok(_mapper.Map<IEnumerable<OrderForResponseDto>>(orders));
        }

        [HttpGet("{orderId}", Name = "GetOrderById")]
        public async Task<ActionResult<OrderForResponseDto>> GetOrderById(string userId, Guid orderId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                return NotFound("This User ID does not exist.");
            }

            var order = await _orderService.FetchOrderByIdAsync(userId, orderId);

            if (order == null)
            {
                return NotFound("This Order ID does not exist.");
            }

            return Ok(_mapper.Map<OrderForResponseDto>(order));
        }

        [HttpPost]
        public async Task<ActionResult<OrderForResponseDto>> CreateOrder(OrderForCreationDto orderDto, string userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await _userProductService.CheckUserExists(userId))
            {
                return NotFound("This User ID does not exist.");
            }

            var cart = await _shoppingCartService.FetchShoppingCartByUserId(userId);

            if (cart == null || !cart.Items.Any())
            {
                return BadRequest("Empty shopping cart.");
            }
            
            var order = _mapper.Map<Order>(cart);
            order.Address = orderDto.Address;

            var createdOrder = await _orderService.CreateOrderAsync(order);

            if (createdOrder == null)
            {
                return BadRequest("Failed to create order.");
            }

            var orderResponseDto = _mapper.Map<OrderForResponseDto>(createdOrder);

            return CreatedAtAction("GetOrderById", new { userId = createdOrder.BuyerId, orderId = createdOrder.Id }, orderResponseDto);
        }
        [HttpDelete("{orderId}")]
        public async Task<ActionResult> DeleteOrder(string userId, Guid orderId)
        {
            var order = await _orderService.FetchOrderByIdAsync(userId, orderId);

            if (order == null)
            {
                return NotFound("This Order ID does not exist");
            }

            var result = await _orderService.RemoveOrder(order);

            if (result.Succeeded)
            {
                return NoContent();
            }
            else
            {
                return BadRequest(result.Error);
            }
        }

        [HttpPatch("{orderId}")]
        public async Task<ActionResult> UpdateOrder(string userId, Guid orderId, [FromBody] JsonPatchDocument<OrderForUpdateDto> patchDocument)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                return NotFound("This User ID does not exist.");
            }

            var order = await _orderService.FetchOrderByIdAsync(userId, orderId);

            if (order == null)
            {
                return NotFound("This Order ID does not exist.");
            }

            var orderToPatch = _mapper.Map<OrderForUpdateDto>(order);

            patchDocument.ApplyTo(orderToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _mapper.Map(orderToPatch, order);

            var updatedOrder = await _orderService.UpdateOrderAsync(order);

            if (updatedOrder == null)
            {
                return BadRequest("Failed to update order.");
            }

            return NoContent();
        }
    }
}
