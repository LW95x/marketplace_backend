using AutoMapper;
using Marketplace.BusinessLayer;
using Marketplace.Controllers;
using Marketplace.DataAccess.Entities;
using Marketplace.DataAccess.Services;
using Marketplace.Helpers;
using Marketplace.MapperProfiles;
using Marketplace.Models;
using Marketplace.Test.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Marketplace.Test
{
    public class ShoppingCartTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IShoppingCartService> _mockShoppingCartService;
        private readonly Mock<IShoppingCartRepository> _mockShoppingCartRepository;
        private readonly Mock<IUserProductService> _mockUserProductService;
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ILogger<ShoppingCartController>> _mockControllerLogger;
        private readonly Mock<ILogger<ShoppingCartService>> _mockServiceLogger;
        private readonly List<User> _testUsers;
        private readonly List<Product> _testProducts;
        private readonly List<ShoppingCart> _testShoppingCarts;
        private readonly ShoppingCartController _controller;
        private readonly ShoppingCartService _service;

        public ShoppingCartTests()
        {
            var config = new MapperConfiguration(c => c.AddProfile<ShoppingCartProfile>());
            _mapper = config.CreateMapper();

            _mockShoppingCartService = new Mock<IShoppingCartService>();
            _mockShoppingCartRepository = new Mock<IShoppingCartRepository>();
            _mockUserProductService = new Mock<IUserProductService>();
            _mockProductService = new Mock<IProductService>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockControllerLogger = new Mock<ILogger<ShoppingCartController>>();
            _mockServiceLogger = new Mock<ILogger<ShoppingCartService>>();

            _testUsers = TestDataFactory.GetUsers();
            _testProducts = TestDataFactory.GetProducts(_testUsers);
            _testShoppingCarts = TestDataFactory.GetShoppingCarts(_testUsers, _testProducts);

            _controller = new ShoppingCartController(_mockShoppingCartService.Object, _mockUserProductService.Object, _mockProductService.Object, _mapper, _mockControllerLogger.Object);
            _service = new ShoppingCartService(_mockProductRepository.Object, _mockShoppingCartRepository.Object, _mockServiceLogger.Object);
        }

        [Fact]
        public async Task GetShoppingCartByUserId_WhenValidUserId_ReturnsOkWithShoppingCart()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var cart = _testShoppingCarts.FirstOrDefault(c => c.BuyerId == userId);

            Assert.NotNull(cart);
            var cartId = cart.Id;

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);

            _mockShoppingCartService.Setup(service => service.FetchShoppingCartByUserId(userId)).ReturnsAsync(cart);

            // Act
            var result = await _controller.GetShoppingCartByUserId(userId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ShoppingCartForResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedCart = Assert.IsAssignableFrom<ShoppingCartForResponseDto>(okResult.Value);

            Assert.Equal(Guid.Parse(userId), returnedCart.BuyerId);
            Assert.Equal(cartId, returnedCart.CartId);
            Assert.Equal(cart.Items.Count, returnedCart.Items.Count);
            Assert.Equal(cart.TotalPrice, returnedCart.TotalPrice);
        }

        [Fact]
        public async Task GetShoppingCartByUserId_WhenUserDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var userId = "notAnId";

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(false);
            _mockShoppingCartService.Setup(service => service.FetchShoppingCartByUserId(userId)).ReturnsAsync((ShoppingCart)null!);

            // Act
            var result = await _controller.GetShoppingCartByUserId(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("This User ID does not exist.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetSingleShoppingCartItem_WhenUserAndCartItemExists_ReturnsOkWithShoppingCartItem()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var cartItemId = _testShoppingCarts[0].Items.First().Id;
            var cartItem = _testShoppingCarts[0].Items.First();

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);

            _mockShoppingCartService.Setup(service => service.FetchSingleShoppingCartItem(userId, cartItemId)).ReturnsAsync(cartItem);

            // Act
            var result = await _controller.GetSingleShoppingCartItem(userId, cartItemId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ShoppingCartItemForResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedCartItem = Assert.IsAssignableFrom<ShoppingCartItemForResponseDto>(okResult.Value);

            Assert.Equal(cartItemId, returnedCartItem.CartItemId);
        }

        [Fact]
        public async Task GetSingleShoppingCartItem_WhenUserDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var userId = "notAnId";
            var cartItemId = _testShoppingCarts[0].Items.First().Id;

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(false);

            _mockShoppingCartService.Setup(service => service.FetchSingleShoppingCartItem(userId, cartItemId)).ReturnsAsync((ShoppingCartItem)null!);

            // Act
            var result = await _controller.GetSingleShoppingCartItem(userId, cartItemId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("This User ID does not exist.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetSingleShoppingCartItem_WhenCartItemDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var cartItemId = Guid.NewGuid();

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);

            _mockShoppingCartService.Setup(service => service.FetchSingleShoppingCartItem(userId, cartItemId)).ReturnsAsync((ShoppingCartItem)null!);

            // Act
            var result = await _controller.GetSingleShoppingCartItem(userId, cartItemId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("This Shopping Cart Item ID does not exist.", notFoundResult.Value);
        }

        [Fact]
        public async Task AddProductToShoppingCart_WhenValidRequest_ReturnsCreated()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var productId = _testProducts[4].Id;
            var product = _testProducts[4];
            var userCart = _testShoppingCarts[0];

            var cartItemCreationDto = new ShoppingCartItemForCreationDto()
            {
                Quantity = 1,
                ProductId = productId
            };

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);

            _mockShoppingCartService.Setup(service => service.FetchShoppingCartByUserId(userId)).ReturnsAsync(userCart);

            _mockProductService.Setup(service => service.FetchProductByIdAsync(productId)).ReturnsAsync(product);

            var shoppingCartItem = _mapper.Map<ShoppingCartItem>(cartItemCreationDto);
            shoppingCartItem.ShoppingCartId = userCart.Id;
            shoppingCartItem.Price = product.Price;

            _mockShoppingCartService.Setup(service => service.AddShoppingCartItem(It.IsAny<ShoppingCartItem>(), userId)).ReturnsAsync(shoppingCartItem);

            var expectedCartItemResponseDto = _mapper.Map<ShoppingCartItemForResponseDto>(shoppingCartItem);

            // Act
            var result = await _controller.AddProductToShoppingCart(cartItemCreationDto, userId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ShoppingCartItemForResponseDto>>(result);
            var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            Assert.Equal(201, createdResult.StatusCode);

            var actualResult = Assert.IsType<ShoppingCartItemForResponseDto>(createdResult.Value);

            Assert.Equal(expectedCartItemResponseDto.CartItemId, actualResult.CartItemId);
            Assert.Equal(expectedCartItemResponseDto.Quantity, actualResult.Quantity);
            Assert.Equal(expectedCartItemResponseDto.TotalPrice, actualResult.TotalPrice);
            Assert.Equal(expectedCartItemResponseDto.Price, actualResult.Price);
            Assert.Equal(expectedCartItemResponseDto.ProductId, actualResult.ProductId);
        }

        [Fact]
        public async Task AddProductToShoppingCart_WhenProductAlreadyExistsInCart_ReturnsNoContent()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var productId = _testProducts[0].Id;
            var product = _testProducts[0];
            var userCart = _testShoppingCarts[0];
            var existingCartItem = _testShoppingCarts[0].Items.FirstOrDefault(c => c.ProductId == productId);
            Assert.NotNull(existingCartItem);

            var cartItemCreationDto = new ShoppingCartItemForCreationDto()
            {
                Quantity = 1,
                ProductId = productId
            };

            var updatedCartItem = _testShoppingCarts[0].Items.FirstOrDefault(c => c.ProductId == productId);
            Assert.NotNull(updatedCartItem);
            updatedCartItem.Quantity = 1;

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);

            _mockShoppingCartService.Setup(service => service.FetchShoppingCartByUserId(userId)).ReturnsAsync(userCart);

            _mockProductService.Setup(service => service.FetchProductByIdAsync(productId)).ReturnsAsync(product);

            _mockShoppingCartService.Setup(service => service.FetchSingleShoppingCartItem(userId, existingCartItem.Id)).ReturnsAsync(existingCartItem);  

            _mockShoppingCartService.Setup(service => service.UpdateShoppingCartItemQuantity(existingCartItem, userId)).ReturnsAsync(updatedCartItem);


            // Act
            var result = await _controller.AddProductToShoppingCart(cartItemCreationDto, userId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ShoppingCartItemForResponseDto>>(result);
            var noContentResult = Assert.IsType<NoContentResult>(actionResult.Result);
            Assert.Equal(204, noContentResult.StatusCode);

            var cartItemAfterUpdate = await _controller.GetSingleShoppingCartItem(userId, existingCartItem.Id);

            var responseActionResult = Assert.IsType<ActionResult<ShoppingCartItemForResponseDto>>(cartItemAfterUpdate);
            var okResult = Assert.IsType<OkObjectResult>(responseActionResult.Result);
            var returnedCartItem = Assert.IsAssignableFrom<ShoppingCartItemForResponseDto>(okResult.Value);

            Assert.Equal(2, returnedCartItem.Quantity);
            // Veryifying that the updated Cart Item contains the correct quantity.
        }

        [Fact]
        public async Task AddProductToShoppingCart_WhenCartItemQuantityExceedsAvailableProductQuantity_ReturnsBadRequest()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var productId = _testProducts[4].Id;
            var product = _testProducts[4];
            var userCart = _testShoppingCarts[0];

            var cartItemCreationDto = new ShoppingCartItemForCreationDto()
            {
                Quantity = 25, // Product max quantity is 20.
                ProductId = productId
            };

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);

            _mockShoppingCartService.Setup(service => service.FetchShoppingCartByUserId(userId)).ReturnsAsync(userCart);

            _mockProductService.Setup(service => service.FetchProductByIdAsync(productId)).ReturnsAsync(product);

            var shoppingCartItem = _mapper.Map<ShoppingCartItem>(cartItemCreationDto);
            shoppingCartItem.ShoppingCartId = userCart.Id;
            shoppingCartItem.Price = product.Price;

            _mockShoppingCartService.Setup(service => service.AddShoppingCartItem(It.IsAny<ShoppingCartItem>(), userId)).ReturnsAsync(shoppingCartItem);

            var expectedCartItemResponseDto = _mapper.Map<ShoppingCartItemForResponseDto>(shoppingCartItem);

            // Act
            var result = await _controller.AddProductToShoppingCart(cartItemCreationDto, userId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ShoppingCartItemForResponseDto>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("This quantity exceeds the maximum available stock.", badRequestResult.Value);
        }

        [Fact]
        public async Task AddProductToShoppingCart_WhenUserDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var userId = "notAnId";
            var productId = _testProducts[4].Id;
            var product = _testProducts[4];

            var cartItemCreationDto = new ShoppingCartItemForCreationDto()
            {
                Quantity = 1,
                ProductId = productId
            };

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(false);

            _mockShoppingCartService.Setup(service => service.FetchShoppingCartByUserId(userId)).ReturnsAsync((ShoppingCart)null!);

            _mockProductService.Setup(service => service.FetchProductByIdAsync(productId)).ReturnsAsync(product);

            var shoppingCartItem = _mapper.Map<ShoppingCartItem>(cartItemCreationDto);
            shoppingCartItem.Price = product.Price;

            _mockShoppingCartService.Setup(service => service.AddShoppingCartItem(It.IsAny<ShoppingCartItem>(), userId)).ReturnsAsync(shoppingCartItem);

            // Act
            var result = await _controller.AddProductToShoppingCart(cartItemCreationDto, userId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ShoppingCartItemForResponseDto>>(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("This User ID does not exist.", notFoundResult.Value);
        }

        [Fact]
        public async Task AddProductToShoppingCart_WhenCartDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var productId = _testProducts[4].Id;
            var product = _testProducts[4];

            var cartItemCreationDto = new ShoppingCartItemForCreationDto()
            {
                Quantity = 1,
                ProductId = productId
            };

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);

            _mockShoppingCartService.Setup(service => service.FetchShoppingCartByUserId(userId)).ReturnsAsync((ShoppingCart)null!);

            _mockProductService.Setup(service => service.FetchProductByIdAsync(productId)).ReturnsAsync(product);

            var shoppingCartItem = _mapper.Map<ShoppingCartItem>(cartItemCreationDto);
            shoppingCartItem.Price = product.Price;

            _mockShoppingCartService.Setup(service => service.AddShoppingCartItem(It.IsAny<ShoppingCartItem>(), userId)).ReturnsAsync(shoppingCartItem);

            // Act
            var result = await _controller.AddProductToShoppingCart(cartItemCreationDto, userId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ShoppingCartItemForResponseDto>>(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("Shopping Cart could not be found.", notFoundResult.Value);
        }

        [Fact]
        public async Task AddProductToShoppingCart_WhenProductDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var productId = Guid.NewGuid();
            var userCart = _testShoppingCarts[0];

            var cartItemCreationDto = new ShoppingCartItemForCreationDto()
            {
                Quantity = 1,
                ProductId = productId
            };

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);

            _mockShoppingCartService.Setup(service => service.FetchShoppingCartByUserId(userId)).ReturnsAsync(userCart);

            _mockProductService.Setup(service => service.FetchProductByIdAsync(productId)).ReturnsAsync((Product)null!);

            var shoppingCartItem = _mapper.Map<ShoppingCartItem>(cartItemCreationDto);

            _mockShoppingCartService.Setup(service => service.AddShoppingCartItem(It.IsAny<ShoppingCartItem>(), userId)).ReturnsAsync(shoppingCartItem);

            // Act
            var result = await _controller.AddProductToShoppingCart(cartItemCreationDto, userId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ShoppingCartItemForResponseDto>>(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("Product ID could not be found.", notFoundResult.Value);
        }

        [Fact]
        public async Task AddProductToShoppingCart_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var productId = _testProducts[4].Id;
            var product = _testProducts[4];
            var userCart = _testShoppingCarts[0];

            var cartItemCreationDto = new ShoppingCartItemForCreationDto()
            {
                Quantity = 1,
                ProductId = productId
            };

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);

            _mockShoppingCartService.Setup(service => service.FetchShoppingCartByUserId(userId)).ReturnsAsync(userCart);

            _mockProductService.Setup(service => service.FetchProductByIdAsync(productId)).ReturnsAsync(product);

            var shoppingCartItem = _mapper.Map<ShoppingCartItem>(cartItemCreationDto);
            shoppingCartItem.ShoppingCartId = userCart.Id;
            shoppingCartItem.Price = product.Price;

            _mockShoppingCartService.Setup(service => service.AddShoppingCartItem(It.IsAny<ShoppingCartItem>(), userId)).ReturnsAsync((ShoppingCartItem)null!);

            var expectedCartItemResponseDto = _mapper.Map<ShoppingCartItemForResponseDto>(shoppingCartItem);

            // Act
            var result = await _controller.AddProductToShoppingCart(cartItemCreationDto, userId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ShoppingCartItemForResponseDto>>(result);
            var objectResult = result.Result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Failed to add the product to the shopping cart due to an internal server error.", objectResult.Value);
        }

        [Fact]
        public async Task DeleteShoppingCartItem_WhenValidUserAndCartItemId_ReturnsNoContent()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var cartItemId = _testShoppingCarts[0].Items.First().Id;
            var cartItem = _testShoppingCarts[0].Items.First();

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);

            _mockShoppingCartService.Setup(service => service.FetchSingleShoppingCartItem(userId, cartItemId)).ReturnsAsync(cartItem);

            _mockShoppingCartService.Setup(service => service.RemoveShoppingCartItem(cartItem, userId)).ReturnsAsync(Result.Success());


            // Act
            var result = await _controller.DeleteShoppingCartItem(userId, cartItemId);

            // Assert
            var deletedResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, deletedResult.StatusCode);

        }

        [Fact]
        public async Task DeleteShoppingCartItem_WhenInvalidUserId_ReturnsNotFound()
        {
            // Arrange
            var userId = "notAnId";
            var cartItemId = _testShoppingCarts[0].Items.First().Id;
            var cartItem = _testShoppingCarts[0].Items.First();

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(false);

            _mockShoppingCartService.Setup(service => service.FetchSingleShoppingCartItem(userId, cartItemId)).ReturnsAsync((ShoppingCartItem)null!);

            _mockShoppingCartService.Setup(service => service.RemoveShoppingCartItem(cartItem, userId)).ReturnsAsync(Result.Fail("This User ID does not exist."));

            // Act
            var result = await _controller.DeleteShoppingCartItem(userId, cartItemId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteShoppingCartItem_WhenInvalidShoppingCartItemId_ReturnsNotFound()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var cartItemId = Guid.NewGuid();

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);

            _mockShoppingCartService.Setup(service => service.FetchSingleShoppingCartItem(userId, cartItemId)).ReturnsAsync((ShoppingCartItem)null!);

            _mockShoppingCartService.Setup(service => service.RemoveShoppingCartItem(null!, userId)).ReturnsAsync(Result.Fail("This Shopping Cart Item ID does not exist."));

            // Act
            var result = await _controller.DeleteShoppingCartItem(userId, cartItemId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteShoppingCartItem_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var cartItemId = _testShoppingCarts[0].Items.First().Id;
            var cartItem = _testShoppingCarts[0].Items.First();

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);

            _mockShoppingCartService.Setup(service => service.FetchSingleShoppingCartItem(userId, cartItemId)).ReturnsAsync(cartItem);

            _mockShoppingCartService.Setup(service => service.RemoveShoppingCartItem(cartItem, userId)).ReturnsAsync(Result.Fail("An internal server error occurred while deleting the Shopping Cart Item."));


            // Act
            var result = await _controller.DeleteShoppingCartItem(userId, cartItemId);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("An internal server error occurred while deleting the Shopping Cart Item.", objectResult.Value);
        }

        [Fact]
        public async Task UpdateShoppingCartItemQuantity_WhenValidRequest_ReturnsNoContent()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var cartItemId = _testShoppingCarts[0].Items.First().Id;
            var cartItem = _testShoppingCarts[0].Items.First();
            var productId = _testProducts[0].Id;
            var product = _testProducts[0];
            var newQuantity = 5;

            var updatedCartItem = _testShoppingCarts[0].Items.FirstOrDefault(c => c.ProductId == productId);
            Assert.NotNull(updatedCartItem);
            updatedCartItem.Quantity = newQuantity; // Original quantity is 1.

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);

            _mockShoppingCartService.Setup(service => service.FetchSingleShoppingCartItem(userId, cartItemId)).ReturnsAsync(cartItem);

            _mockProductService.Setup(service => service.FetchProductByIdAsync(productId)).ReturnsAsync(product);

            _mockShoppingCartService.Setup(service => service.UpdateShoppingCartItemQuantity(cartItem, userId)).ReturnsAsync(updatedCartItem);

            // Act
            var result = await _controller.UpdateShoppingCartItemQuantity(userId, cartItemId, newQuantity);

            // Assert
            var updatedResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, updatedResult.StatusCode);

            var cartItemAfterUpdate = await _controller.GetSingleShoppingCartItem(userId, cartItemId);

            var actionResult = Assert.IsType<ActionResult<ShoppingCartItemForResponseDto>>(cartItemAfterUpdate);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedCartItem = Assert.IsAssignableFrom<ShoppingCartItemForResponseDto>(okResult.Value);

            Assert.Equal(newQuantity, returnedCartItem.Quantity);
            // Veryifying that the updated Cart Item contains the correct quantity.
        }

        [Fact]
        public async Task UpdateShoppingCartItemQuantity_WhenCartItemQuantityExceedsProductsAvailableQuantity_ReturnsBadRequest()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var cartItemId = _testShoppingCarts[0].Items.First().Id;
            var cartItem = _testShoppingCarts[0].Items.First();
            var productId = _testProducts[0].Id;
            var product = _testProducts[0];
            var newQuantity = 50;

            var updatedCartItem = _testShoppingCarts[0].Items.FirstOrDefault(c => c.ProductId == productId);
            Assert.NotNull(updatedCartItem);
            updatedCartItem.Quantity = newQuantity; // Product's maximum available quantity is 10.

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);

            _mockShoppingCartService.Setup(service => service.FetchSingleShoppingCartItem(userId, cartItemId)).ReturnsAsync(cartItem);

            _mockProductService.Setup(service => service.FetchProductByIdAsync(productId)).ReturnsAsync(product);

            _mockShoppingCartService.Setup(service => service.UpdateShoppingCartItemQuantity(cartItem, userId)).ReturnsAsync(updatedCartItem);

            // Act
            var result = await _controller.UpdateShoppingCartItemQuantity(userId, cartItemId, newQuantity);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("This quantity exceeds the maximum available stock.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateShoppingCartItemQuantity_WhenUserDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var userId = "notAnId";
            var cartItemId = _testShoppingCarts[0].Items.First().Id;
            var cartItem = _testShoppingCarts[0].Items.First();
            var productId = _testProducts[0].Id;
            var product = _testProducts[0];
            var newQuantity = 5;

            var updatedCartItem = _testShoppingCarts[0].Items.FirstOrDefault(c => c.ProductId == productId);
            Assert.NotNull(updatedCartItem);
            updatedCartItem.Quantity = newQuantity; // Original quantity is 1.

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(false);

            _mockShoppingCartService.Setup(service => service.FetchSingleShoppingCartItem(userId, cartItemId)).ReturnsAsync((ShoppingCartItem)null!);

            _mockProductService.Setup(service => service.FetchProductByIdAsync(productId)).ReturnsAsync(product);

            _mockShoppingCartService.Setup(service => service.UpdateShoppingCartItemQuantity(cartItem, userId)).ReturnsAsync((ShoppingCartItem)null!);

            // Act
            var result = await _controller.UpdateShoppingCartItemQuantity(userId, cartItemId, newQuantity);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("User ID could not be found.", notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateShoppingCartItemQuantity_WhenCartItemDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var cartItemId = Guid.NewGuid();
            var productId = _testProducts[0].Id;
            var product = _testProducts[0];
            var newQuantity = 5;

            var updatedCartItem = _testShoppingCarts[0].Items.FirstOrDefault(c => c.ProductId == productId);
            Assert.NotNull(updatedCartItem);
            updatedCartItem.Quantity = newQuantity; // Original quantity is 1.

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);

            _mockShoppingCartService.Setup(service => service.FetchSingleShoppingCartItem(userId, cartItemId)).ReturnsAsync((ShoppingCartItem)null!);

            _mockProductService.Setup(service => service.FetchProductByIdAsync(productId)).ReturnsAsync(product);

            _mockShoppingCartService.Setup(service => service.UpdateShoppingCartItemQuantity(null!, userId)).ReturnsAsync((ShoppingCartItem)null!);

            // Act
            var result = await _controller.UpdateShoppingCartItemQuantity(userId, cartItemId, newQuantity);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("This Shopping Cart Item ID does not exist.", notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateShoppingCartItemQuantity_WhenProductDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var cartItemId = _testShoppingCarts[0].Items.First().Id;
            var cartItem = _testShoppingCarts[0].Items.First();
            var productId = Guid.NewGuid();
            var newQuantity = 5;

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);

            _mockShoppingCartService.Setup(service => service.FetchSingleShoppingCartItem(userId, cartItemId)).ReturnsAsync(cartItem);

            _mockProductService.Setup(service => service.FetchProductByIdAsync(productId)).ReturnsAsync((Product)null!);

            _mockShoppingCartService.Setup(service => service.UpdateShoppingCartItemQuantity(null!, userId)).ReturnsAsync((ShoppingCartItem)null!);

            // Act
            var result = await _controller.UpdateShoppingCartItemQuantity(userId, cartItemId, newQuantity);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("This Product ID could not be found.", notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateShoppingCartItemQuantity_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var cartItemId = _testShoppingCarts[0].Items.First().Id;
            var cartItem = _testShoppingCarts[0].Items.First();
            var productId = _testProducts[0].Id;
            var product = _testProducts[0];
            var newQuantity = 5;

            var updatedCartItem = _testShoppingCarts[0].Items.FirstOrDefault(c => c.ProductId == productId);
            Assert.NotNull(updatedCartItem);
            updatedCartItem.Quantity = newQuantity; // Original quantity is 1.

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);

            _mockShoppingCartService.Setup(service => service.FetchSingleShoppingCartItem(userId, cartItemId)).ReturnsAsync(cartItem);

            _mockProductService.Setup(service => service.FetchProductByIdAsync(productId)).ReturnsAsync(product);

            _mockShoppingCartService.Setup(service => service.UpdateShoppingCartItemQuantity(cartItem, userId)).ReturnsAsync((ShoppingCartItem)null!);

            // Act
            var result = await _controller.UpdateShoppingCartItemQuantity(userId, cartItemId, newQuantity);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Failed to update shopping cart item due to an internal server error.", objectResult.Value);
        }

        [Fact]
        public async Task FetchShoppingCartByUserId_WhenValidUserId_ReturnsShoppingCart()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var cart = _testShoppingCarts.FirstOrDefault(c => c.BuyerId == userId);

            Assert.NotNull(cart);
            var cartId = cart.Id;

            _mockShoppingCartRepository.Setup(repo => repo.GetShoppingCartByUserId(userId)).ReturnsAsync(cart);

            // Act
            var result = await _service.FetchShoppingCartByUserId(userId);

            // Assert
            var returnedShoppingCart = Assert.IsType<ShoppingCart>(result);
            Assert.Equal(cart.Items.Count(), result.Items.Count());

            foreach(var cartItem in cart.Items)
            {
                Assert.Contains(returnedShoppingCart.Items, i =>
                    i.Id == cartItem.Id &&
                    i.ShoppingCartId == cartItem.ShoppingCartId &&
                    i.ProductId == cartItem.ProductId
                );
            }
        }

        [Fact]
        public async Task FetchSingleShoppingCartItem_WhenValidUserIdAndCartItemId_ReturnsShoppingCartItem()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var cartItem = _testShoppingCarts[0].Items.First();
            var cartItemId = _testShoppingCarts[0].Items.First().Id;

            _mockShoppingCartRepository.Setup(repo => repo.GetSingleShoppingCartItem(userId, cartItemId)).ReturnsAsync(cartItem);

            // Act
            var result = await _service.FetchSingleShoppingCartItem(userId, cartItemId);

            // Assert
            var returnedCartItem = Assert.IsType<ShoppingCartItem>(result);

            Assert.Equal(cartItem.Id, returnedCartItem.Id);
            Assert.Equal(cartItem.ProductId, returnedCartItem.ProductId);
            Assert.Equal(cartItem.Quantity, returnedCartItem.Quantity);
            Assert.Equal(cartItem.Price, returnedCartItem.Price);
            Assert.Equal(cartItem.TotalPrice, returnedCartItem.TotalPrice);
            Assert.Equal(cartItem.ShoppingCartId, returnedCartItem.ShoppingCartId);
        }

        [Fact]
        public async Task AddShoppingCartItem_WhenValidRequest_ReturnsCorrectTotalPriceForItem()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var product = _testProducts[4];
            var cart = _testShoppingCarts.FirstOrDefault(c => c.BuyerId == userId);
            Assert.NotNull(cart);

            var cartItem = new ShoppingCartItem(product.Id, 5)
            {
                Id = Guid.NewGuid(),
                Price = product.Price,
                TotalPrice = product.Price * 5,
                ShoppingCartId = cart.Id
            };

            cart.Items.Add(cartItem);

            _mockShoppingCartRepository.Setup(repo => repo.AddProductToShoppingCart(cartItem, userId)).ReturnsAsync(cartItem);

            _mockShoppingCartRepository.Setup(repo => repo.GetShoppingCartByUserId(userId)).ReturnsAsync(cart);

            _mockShoppingCartRepository.Setup(repo => repo.UpdateCartAsync(cart));

            // Act
            var result = await _service.AddShoppingCartItem(cartItem, userId);

            // Assert
            var returnedCartItem = Assert.IsType<ShoppingCartItem>(result);

            Assert.Equal(cartItem.Id, returnedCartItem.Id);
            Assert.Equal(cartItem.ProductId, returnedCartItem.ProductId);
            Assert.Equal(cartItem.Quantity, returnedCartItem.Quantity);
            Assert.Equal(cartItem.Price, returnedCartItem.Price);
            Assert.Equal(cartItem.ShoppingCartId, returnedCartItem.ShoppingCartId);

            Assert.Equal(cartItem.Price, result.Price, precision: 2);
            Assert.Equal(cartItem.TotalPrice, returnedCartItem.TotalPrice, precision: 2);
        }

        [Fact]
        public async Task AddShoppingCartItem_WhenValidRequest_ReturnsCartTotalPriceCorrectly()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var product = _testProducts[4];
            var cart = new ShoppingCart
            {
                Id = Guid.NewGuid(),
                BuyerId = userId,
                Items = new List<ShoppingCartItem>(),
                TotalPrice = 0
            };

            var cartItem = new ShoppingCartItem(Guid.NewGuid(), 5)
            {
                Id = Guid.NewGuid(),
                Price = 10,
                TotalPrice = 50,
                ShoppingCartId = cart.Id
            };

            cart.Items.Add(cartItem);

            _mockShoppingCartRepository.Setup(repo => repo.AddProductToShoppingCart(cartItem, userId)).ReturnsAsync(cartItem);

            _mockShoppingCartRepository.Setup(repo => repo.GetShoppingCartByUserId(userId)).ReturnsAsync(cart);

            _mockShoppingCartRepository.Setup(repo => repo.UpdateCartAsync(cart));

            // Act
            var result = await _service.AddShoppingCartItem(cartItem, userId);

            // Assert
            Assert.Equal(50, cart.TotalPrice);
        }

        [Fact]
        public async Task RemoveShoppingCartItem_WhenValidRequest_ReturnsCartTotalPriceCorrectly()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var cart = _testShoppingCarts.FirstOrDefault(c => c.BuyerId == userId);
            Assert.NotNull(cart);

            var cartItem = _testShoppingCarts[0].Items.First();
            Assert.NotNull(cartItem);

            _mockShoppingCartRepository.Setup(repo => repo.DeleteShoppingCartItemAsync(cartItem)).ReturnsAsync(Result.Success());

            _mockShoppingCartRepository.Setup(repo => repo.GetShoppingCartByUserId(userId)).ReturnsAsync(cart);

            _mockShoppingCartRepository.Setup(repo => repo.UpdateCartAsync(cart));

            // Act
            var result = await _service.RemoveShoppingCartItem(cartItem, userId);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Null(result.Error);
        }

        [Fact]
        public async Task RemoveShoppingCartItem_WhenValidRequest_ReturnsSuccessResult()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var cart = _testShoppingCarts.FirstOrDefault(c => c.BuyerId == userId);
            Assert.NotNull(cart);
           // Total Price of Cart is 120.

            var cartItem = _testShoppingCarts[0].Items.First();
            Assert.NotNull(cartItem);
            // Price of Cart Item #1 is 50.

            _mockShoppingCartRepository.Setup(repo => repo.DeleteShoppingCartItemAsync(cartItem)).ReturnsAsync(Result.Success());

            _mockShoppingCartRepository.Setup(repo => repo.GetShoppingCartByUserId(userId)).ReturnsAsync(cart);

            _mockShoppingCartRepository.Setup(repo => repo.UpdateCartAsync(It.IsAny<ShoppingCart>())).Callback<ShoppingCart>(updatedCart =>
            {
                updatedCart.TotalPrice = updatedCart.Items.Sum(item => item.TotalPrice);
            });

            // Act
            var result = await _service.RemoveShoppingCartItem(cartItem, userId);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal(cart.Items.Sum(item => item.TotalPrice), cart.TotalPrice);
        }

        [Fact]
        public async Task UpdateShoppingCartItemQuantity_WhenValidRequest_ReturnsCorrectQuantityAndTotalPrice()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var cart = _testShoppingCarts.FirstOrDefault(c => c.BuyerId == userId);
            Assert.NotNull(cart);

            var cartItem = _testShoppingCarts[0].Items.First();
            Assert.NotNull(cartItem);

            var quantity = 5;
            cartItem.Quantity = quantity; // Original quantity was 1.

            _mockShoppingCartRepository.Setup(repo => repo.UpdateShoppingCartItemQuantity(cartItem)).ReturnsAsync(cartItem);

            _mockShoppingCartRepository.Setup(repo => repo.GetShoppingCartByUserId(userId)).ReturnsAsync(cart);

            _mockShoppingCartRepository.Setup(repo => repo.UpdateCartAsync(cart));

            // Act
            var result = await _service.UpdateShoppingCartItemQuantity(cartItem, userId);

            // Assert
            Assert.Equal(5, result.Quantity);
            Assert.Equal(cartItem.Price * quantity, result.TotalPrice);
            Assert.Equal(cart.Items.Sum(item => item.TotalPrice), cart.TotalPrice);
        }
    }
}
