using AutoFixture;
using AutoMapper;
using Marketplace.BusinessLayer;
using Marketplace.Controllers;
using Marketplace.DataAccess.Entities;
using Marketplace.DataAccess.Repositories;
using Marketplace.DataAccess.Services;
using Marketplace.Helpers;
using Marketplace.MapperProfiles;
using Marketplace.Models;
using Marketplace.Test.Fixtures;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Marketplace.Test
{
    public class OrderTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<IUserProductService> _mockUserProductService;
        private readonly Mock<IShoppingCartService> _mockShoppingCartService;
        private readonly Mock<IOrderService> _mockOrderService;
        private readonly Mock<ILogger<OrdersController>> _mockLogger;
        private readonly List<User> _testUsers;
        private readonly List<Product> _testProducts;
        private readonly List<Order> _testOrders;
        private readonly List<ShoppingCart> _testShoppingCarts;
        private readonly OrdersController _controller;
        private readonly OrderService _service;

        public OrderTests()
        {
            var config = new MapperConfiguration(c => c.AddProfile<OrderProfile>());
            _mapper = config.CreateMapper();

            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockUserProductService = new Mock<IUserProductService>();
            _mockShoppingCartService = new Mock<IShoppingCartService>();
            _mockOrderService = new Mock<IOrderService>();
            _mockLogger = new Mock<ILogger<OrdersController>>();

            _testUsers = TestDataFactory.GetUsers();
            _testProducts = TestDataFactory.GetProducts(_testUsers);
            _testOrders = TestDataFactory.GetOrders(_testUsers, _testProducts);
            _testShoppingCarts = TestDataFactory.GetShoppingCarts(_testUsers, _testProducts);

            _controller = new OrdersController(_mockOrderService.Object, _mockUserProductService.Object, _mockShoppingCartService.Object, _mapper, _mockLogger.Object);
            _service = new OrderService(_mockOrderRepository.Object);
        }

        [Fact]
        public async Task GetOrders_WhenValidUserId_ReturnsOkWithAllOrders()
        {
            // Arrange
            var userId = _testUsers[1].Id;
            var userOrders = _testOrders.Where(o => o.BuyerId == userId).ToList();

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);
            _mockOrderService.Setup(service => service.FetchOrdersAsync(userId)).ReturnsAsync(userOrders);

            // Act
            var result = await _controller.GetOrders(userId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<OrderForResponseDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedOrders = Assert.IsAssignableFrom<IEnumerable<OrderForResponseDto>>(okResult.Value);

            Assert.Equal(userOrders.Count, returnedOrders.Count());

            foreach (var order in returnedOrders)
            {
                Assert.Equal(Guid.Parse(userId), order.BuyerId);
            };
        }

        [Fact]
        public async Task GetOrders_WhenUserDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var userId = "notAnId";

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(false);

            // Act
            var result = await _controller.GetOrders(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetOrders_WhenUserExistsWithNoOrders_ReturnsEmptyList()
        {
            // Arrange
            var userId = _testUsers[2].Id;
            var userOrders = _testOrders.Where(o => o.BuyerId == userId).ToList();

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);
            _mockOrderService.Setup(service => service.FetchOrdersAsync(userId)).ReturnsAsync(userOrders);

            // Act
            var result = await _controller.GetOrders(userId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<OrderForResponseDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedOrders = Assert.IsAssignableFrom<IEnumerable<OrderForResponseDto>>(okResult.Value);

            Assert.Empty(returnedOrders);
        }

        [Fact]
        public async Task GetOrderById_WhenValidUserIdAndOrderId_ReturnsOkWithOrder()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var order = _testOrders.FirstOrDefault(o => o.BuyerId == userId);

            Assert.NotNull(order);
            var orderId = order.Id;

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);
            _mockOrderService.Setup(service => service.FetchOrderByIdAsync(userId, orderId)).ReturnsAsync(order);


            // Act
            var result = await _controller.GetOrderById(userId, orderId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<OrderForResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedOrder = Assert.IsAssignableFrom<OrderForResponseDto>(okResult.Value);

            Assert.Equal(Guid.Parse(userId), returnedOrder.BuyerId);
            Assert.Equal(orderId, returnedOrder.OrderId);
        }

        [Fact]
        public async Task GetOrderById_WhenUserDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var userId = "notAnId";
            var orderId = _testOrders[0].Id;

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(false);
            _mockOrderService.Setup(service => service.FetchOrderByIdAsync(userId, orderId)).ReturnsAsync((Order)null!);

            // Act
            var result = await _controller.GetOrderById(userId, orderId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("This User ID does not exist.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetOrderById_WhenOrderDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var orderId = Guid.NewGuid();

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);
            _mockOrderService.Setup(service => service.FetchOrderByIdAsync(userId, orderId)).ReturnsAsync((Order)null!);

            // Act
            var result = await _controller.GetOrderById(userId, orderId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("This Order ID does not exist.", notFoundResult.Value);
        }

        [Fact]
        public async Task CreateOrder_WhenOrderIsValid_ReturnsCreated()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var cart = _testShoppingCarts.FirstOrDefault(o => o.BuyerId == userId);
            Assert.NotNull(cart);

            var orderCreationDto = new OrderForCreationDto
            {
                Address = "Caersws, SY17 5SA"
            };

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);

            _mockShoppingCartService.Setup(service => service.FetchShoppingCartByUserId(userId)).ReturnsAsync(cart);

            var order = _mapper.Map<Order>(cart);
            order.Address = orderCreationDto.Address;

            _mockOrderService.Setup(service => service.CreateOrderAsync(It.IsAny<Order>())).ReturnsAsync(order);


            var expectedResponseDto = _mapper.Map<OrderForResponseDto>(order);

            // Act

            var result = await _controller.CreateOrder(orderCreationDto, userId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<OrderForResponseDto>>(result);
            var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            Assert.Equal(201, createdResult.StatusCode);

            var actualResult = Assert.IsType<OrderForResponseDto>(createdResult.Value);

            Assert.Equal(expectedResponseDto.BuyerId, actualResult.BuyerId);
            Assert.Equal(expectedResponseDto.TotalPrice, actualResult.TotalPrice);
            Assert.Equal(expectedResponseDto.Address, actualResult.Address);
            Assert.Equal(expectedResponseDto.Date, actualResult.Date);
            Assert.Equal(expectedResponseDto.Status, actualResult.Status);
            Assert.Equal(expectedResponseDto.OrderId, actualResult.OrderId);
        }

        [Fact]
        public async Task CreateOrder_WhenInvalidOrder_ReturnsBadRequest()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var cart = new ShoppingCart(); // Empty shopping cart
            Assert.NotNull(cart);
   

            var orderCreationDto = new OrderForCreationDto
            {
                Address = "Caersws, SY17 5SA"
            };

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);

            _mockShoppingCartService.Setup(service => service.FetchShoppingCartByUserId(userId)).ReturnsAsync(cart);

            var order = _mapper.Map<Order>(cart);
            order.Address = orderCreationDto.Address;

            _mockOrderService.Setup(service => service.CreateOrderAsync(It.IsAny<Order>())).ReturnsAsync(order);

            var expectedResponseDto = _mapper.Map<OrderForResponseDto>(order);

            // Act
            var result = await _controller.CreateOrder(orderCreationDto, userId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<OrderForResponseDto>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Empty shopping cart.", badRequestResult.Value);
        }

        [Fact]  
        public async Task CreateOrder_WhenInvalidUserId_ReturnsNotFound() { 
            // Arrange
            var userId = "notAnId";
            var cart = new ShoppingCart();

            var orderCreationDto = new OrderForCreationDto
            {
                Address = "Caersws, SY17 5SA"
            };

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(false);

            _mockShoppingCartService.Setup(service => service.FetchShoppingCartByUserId(userId)).ReturnsAsync(cart);

            var order = _mapper.Map<Order>(cart);
            order.Address = orderCreationDto.Address;

            _mockOrderService.Setup(service => service.CreateOrderAsync(It.IsAny<Order>())).ReturnsAsync(order);

            var expectedResponseDto = _mapper.Map<OrderForResponseDto>(order);

            // Act
            var result = await _controller.CreateOrder(orderCreationDto, userId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<OrderForResponseDto>>(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("This User ID does not exist.", notFoundResult.Value);
        }

        [Fact]  
        public async Task CreateOrder_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var cart = _testShoppingCarts.FirstOrDefault(o => o.BuyerId == userId);
            Assert.NotNull(cart);

            var orderCreationDto = new OrderForCreationDto
            {
                Address = "Caersws, SY17 5SA"
            };

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);

            _mockShoppingCartService.Setup(service => service.FetchShoppingCartByUserId(userId)).ReturnsAsync(cart);

            var order = _mapper.Map<Order>(cart);
            order.Address = orderCreationDto.Address;

            _mockOrderService.Setup(service => service.CreateOrderAsync(It.IsAny<Order>())).ReturnsAsync((Order)null!);

            var expectedResponseDto = _mapper.Map<OrderForResponseDto>(order);


            // Act
            var result = await _controller.CreateOrder(orderCreationDto, userId);

            // Assert
            Assert.IsType<ActionResult<OrderForResponseDto>>(result);
            var objectResult = result.Result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Failed to create the order due to an internal server error.", objectResult.Value);
        }

        [Fact]
        public async Task DeleteOrder_WhenValidUserAndOrderId_ReturnsNoContent()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var order = _testOrders.FirstOrDefault(o => o.BuyerId == userId);
            Assert.NotNull(order);
            var orderId = order.Id;

            _mockOrderService.Setup(service => service.FetchOrderByIdAsync(userId, orderId)).ReturnsAsync(order);

            _mockOrderService.Setup(service => service.RemoveOrder(order)).ReturnsAsync(Result.Success());


            // Act
            var result = await _controller.DeleteOrder(userId, orderId);

            // Assert
            var deletedResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, deletedResult.StatusCode);
        }

        [Fact]
        public async Task DeleteOrder_WhenInvalidOrderId_ReturnsNotFound()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var orderId = Guid.NewGuid();

            _mockOrderService.Setup(service => service.FetchOrderByIdAsync(userId, orderId)).ReturnsAsync((Order)null!);

            _mockOrderService.Setup(service => service.RemoveOrder(It.IsAny<Order>())).ReturnsAsync(Result.Fail("This Order ID does not exist"));

            // Act
            var result = await _controller.DeleteOrder(userId, orderId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteOrder_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var order = _testOrders.FirstOrDefault(o => o.BuyerId == userId);
            Assert.NotNull(order);
            var orderId = order.Id;

            _mockOrderService.Setup(service => service.FetchOrderByIdAsync(userId, orderId)).ReturnsAsync(order);

            _mockOrderService.Setup(service => service.RemoveOrder(It.IsAny<Order>())).ReturnsAsync(Result.Fail("An internal server error occurred while deleting the order."));

            // Act
            var result = await _controller.DeleteOrder(userId, orderId);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("An internal server error occurred while deleting the order.", objectResult.Value);
        }

        [Fact]
        public async Task UpdateOrder_WhenValidRequest_ReturnsNoContent()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var order = _testOrders.FirstOrDefault(o => o.BuyerId == userId);
            Assert.NotNull(order);
            var orderId = order.Id;

            var updatedStatus = OrderStatus.Completed;
            // First order is defined as Pending originally.
            var updatedAddress = "54, Blackstock Street, Greater Manchester, M13 0FZ";

            var patchDocument = new JsonPatchDocument<OrderForUpdateDto>();
            patchDocument.Replace(o => o.Status, updatedStatus);
            patchDocument.Replace(o => o.Address, updatedAddress);

            var updatedOrder = _testOrders.FirstOrDefault(o => o.BuyerId == userId);
            Assert.NotNull(updatedOrder);
            updatedOrder.Status = updatedStatus;
            updatedOrder.Address = updatedAddress;

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);

            _mockOrderService.Setup(service => service.FetchOrderByIdAsync(userId, orderId)).ReturnsAsync(order);

            _mockOrderService.Setup(service => service.UpdateOrderAsync(It.IsAny<Order>())).Callback<Order>(o =>
            {
                o.Status = updatedStatus;
                o.Address = updatedAddress;
            })
                .ReturnsAsync(updatedOrder);

            // Act
            var result = await _controller.UpdateOrder(userId, orderId, patchDocument);

            // Assert
            var updatedResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, updatedResult.StatusCode);
        }

        [Fact]
        public async Task UpdateOrder_WhenInvalidRequestBody_ReturnsBadRequest()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var order = _testOrders.FirstOrDefault(o => o.BuyerId == userId);
            Assert.NotNull(order);
            var orderId = order.Id;

            var fixture = new Fixture();
            var updatedAddress = new string(fixture.CreateMany<char>(256).ToArray()); 

            var patchDocument = new JsonPatchDocument<OrderForUpdateDto>();
            patchDocument.Replace(o => o.Address, updatedAddress);

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);

            _mockOrderService.Setup(service => service.FetchOrderByIdAsync(userId, orderId)).ReturnsAsync(order);

            _controller.ModelState.AddModelError("Address", "Address must contain less than 255 characters.");

            // Act
            var result = await _controller.UpdateOrder(userId, orderId, patchDocument);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);

            var modelState = badRequestResult.Value as SerializableError;
            Assert.NotNull(modelState);
            Assert.True(modelState.ContainsKey("Address"));

            var addressError = modelState["Address"] as string[];
            Assert.NotNull(addressError);
            Assert.Contains("Address must contain less than 255 characters.", addressError);
        }

        [Fact]
        public async Task UpdateOrder_WhenInvalidUser_ReturnsNotFound()
        {
            // Arrange
            var userId = "notAnId";
            var orderId = _testOrders[0].Id;

            var updatedStatus = OrderStatus.Completed;
            var patchDocument = new JsonPatchDocument<OrderForUpdateDto>();
            patchDocument.Replace(o => o.Status, updatedStatus);

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateOrder(userId, orderId, patchDocument);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("This User ID does not exist.", notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateOrder_WhenInvalidOrder_ReturnsNotFound()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var orderId = Guid.NewGuid();

            var updatedStatus = OrderStatus.Completed;
            var patchDocument = new JsonPatchDocument<OrderForUpdateDto>();
            patchDocument.Replace(o => o.Status, updatedStatus);

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);

            _mockOrderService.Setup(service => service.FetchOrderByIdAsync(userId, orderId)).ReturnsAsync((Order)null!);

            // Act
            var result = await _controller.UpdateOrder(userId, orderId, patchDocument);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("This Order ID does not exist.", notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateOrder_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var order = _testOrders.FirstOrDefault(o => o.BuyerId == userId);
            Assert.NotNull(order);
            var orderId = order.Id;

            var updatedStatus = OrderStatus.Completed;

            var patchDocument = new JsonPatchDocument<OrderForUpdateDto>();
            patchDocument.Replace(o => o.Status, updatedStatus);

            var updatedOrder = _testOrders.FirstOrDefault(o => o.BuyerId == userId);
            Assert.NotNull(updatedOrder);
            updatedOrder.Status = updatedStatus;

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);

            _mockOrderService.Setup(service => service.FetchOrderByIdAsync(userId, orderId)).ReturnsAsync(order);

            _mockOrderService.Setup(service => service.UpdateOrderAsync(It.IsAny<Order>()))
                .ReturnsAsync((Order)null!);

            // Act
            var result = await _controller.UpdateOrder(userId, orderId, patchDocument);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Failed to update the order due to an internal server error.", objectResult.Value);
        }

        [Fact]  
        public async Task FetchOrdersAsync_WhenValidUserId_ReturnsListOfUserOrders()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var userOrders = _testOrders.Where(o => o.BuyerId == userId).ToList();

            _mockOrderRepository.Setup(repo => repo.GetOrdersAsync(userId)).ReturnsAsync(userOrders);

            // Act
            var result = await _service.FetchOrdersAsync(userId);

            // Assert
            var returnedOrders = Assert.IsAssignableFrom<IEnumerable<Order>>(result);
            Assert.Equal(userOrders.Count(), returnedOrders.Count());
            Assert.IsType<List<Order>>(result);

            foreach(var order in userOrders)
            {
                Assert.Contains(returnedOrders, o =>
                    o.Id == order.Id &&
                    o.BuyerId != null &&
                    o.BuyerId == order.BuyerId
                );
            }
        }

        [Fact]
        public async Task FetchOrderByIdAsync_WhenValidUserAndOrderId_ReturnsCorrectOrder()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var order = _testOrders.FirstOrDefault(o => o.BuyerId == userId);

            Assert.NotNull(order);
            var orderId = order.Id;

            _mockOrderRepository.Setup(repo => repo.GetOrderByIdAsync(userId, orderId)).ReturnsAsync(order);


            // Act
            var result = await _service.FetchOrderByIdAsync(userId, orderId);

            // Assert
            var returnedOrder = Assert.IsType<Order>(result);

            Assert.Equal(order.Id, returnedOrder.Id);
            Assert.Equal(order.BuyerId, returnedOrder.BuyerId);
            Assert.Equal(order.TotalPrice, returnedOrder.TotalPrice);
            Assert.Equal(order.Date, returnedOrder.Date);
            Assert.Equal(order.Status, returnedOrder.Status);
            Assert.Equal(order.Address, returnedOrder.Address);
            Assert.Equal(order.OrderItems.Count, returnedOrder.OrderItems.Count);
            Assert.True(order.OrderItems.SequenceEqual(returnedOrder.OrderItems));
        }

        [Fact]
        public async Task FetchOrderByIdAsync_WhenInvalidOrderId_ReturnsNull()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var orderId = Guid.NewGuid();

            _mockOrderRepository.Setup(repo => repo.GetOrderByIdAsync(userId, orderId)).ReturnsAsync((Order)null!);

            // Act
            var result = await _service.FetchOrderByIdAsync(userId, orderId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateOrderAsync_WhenValidRequest_ReturnsCreatedOrder()
        {
            // Arrange
            var order = _testOrders[0];

            _mockOrderRepository.Setup(repo => repo.AddOrderAsync(order)).ReturnsAsync(order);

            // Act
            var result = await _service.CreateOrderAsync(order);
            
            // Assert
            var returnedOrder = Assert.IsType<Order>(result);
            Assert.Equal(order.Id, returnedOrder.Id);
            Assert.Equal(order.BuyerId, returnedOrder.BuyerId);
            Assert.Equal(order.TotalPrice, returnedOrder.TotalPrice);
            Assert.Equal(order.Date, returnedOrder.Date);
            Assert.Equal(order.Status, returnedOrder.Status);
            Assert.Equal(order.Address, returnedOrder.Address);
            Assert.Equal(order.OrderItems.Count, returnedOrder.OrderItems.Count);
            Assert.True(order.OrderItems.SequenceEqual(returnedOrder.OrderItems));
        }

        [Fact]
        public async Task RemoveOrder_WhenValidDeletion_ReturnsSuccessResult()
        {
            // Arrange
            var order = _testOrders[0];

            _mockOrderRepository.Setup(repo => repo.DeleteOrderAsync(order)).ReturnsAsync(Result.Success());

            // Act
            var result = await _service.RemoveOrder(order);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Null(result.Error);
        }

        [Fact]  
        public async Task RemoveOrder_WhenDeletionFails_ReturnsFailResult()
        {
            // Arrange
            var order = _testOrders[0];

            _mockOrderRepository.Setup(repo => repo.DeleteOrderAsync(order)).ReturnsAsync(Result.Fail("An internal server error occurred while deleting the order."));

            // Act
            var result = await _service.RemoveOrder(order);

            // Assert
            Assert.Equal("An internal server error occurred while deleting the order.", result.Error);
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async Task UpdateOrderAsync_WhenValidUpdate_ReturnsUpdatedOrder()
        {
            // Arrange
            var order = _testOrders[0];
            order.Status = OrderStatus.Completed; // Original status was pending.

            _mockOrderRepository.Setup(repo => repo.UpdateOrderAsync(order)).ReturnsAsync(order);

            // Act
            var result = await _service.UpdateOrderAsync(order);

            // Assert
            var returnedOrder = Assert.IsType<Order>(result);
            Assert.Equal(OrderStatus.Completed, returnedOrder.Status);
        }
    }
}
