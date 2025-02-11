using AutoMapper;
using Azure;
using Marketplace.BusinessLayer;
using Marketplace.Controllers;
using Marketplace.DataAccess.DbContexts;
using Marketplace.DataAccess.Entities;
using Marketplace.DataAccess.Repositories;
using Marketplace.Helpers;
using Marketplace.MapperProfiles;
using Marketplace.Models;
using Marketplace.Test.Fixtures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;

namespace Marketplace.Test
{
    public class UserProductTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IUserProductRepository> _mockUserProductRepository;
        private readonly Mock<IUserProductService> _mockUserProductService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ILogger<UserProductsController>> _mockLogger;
        private readonly List<User> _testUsers;
        private readonly List<Product> _testProducts;
        private readonly UserProductsController _controller;
        private readonly UserProductService _service;

        public UserProductTests()
        { 
            var config = new MapperConfiguration(c => c.AddProfile<ProductProfile>());
            _mapper = config.CreateMapper();

            _mockUserProductRepository = new Mock<IUserProductRepository>();
            _mockUserProductService = new Mock<IUserProductService>();
            _mockUserService = new Mock<IUserService>();
            _mockLogger = new Mock<ILogger<UserProductsController>>();

            _testUsers = TestDataFactory.GetUsers();
            _testProducts = TestDataFactory.GetProducts(_testUsers);

            _controller = new UserProductsController(_mapper, _mockUserProductService.Object, _mockUserService.Object, _mockLogger.Object);
            _service = new UserProductService(_mockUserProductRepository.Object);
        }

        [Fact]
        public async Task GetUserProducts_UserExists_ReturnsOkWithProducts()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var userProducts = _testProducts.Where(p => p.SellerId == userId).ToList();

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);
            _mockUserProductService.Setup(service => service.FetchUserProductsAsync(userId)).ReturnsAsync(userProducts);

            // Act
            var result = await _controller.GetUserProducts(userId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<ProductForResponseDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedProducts = Assert.IsAssignableFrom<IEnumerable<ProductForResponseDto>>(okResult.Value);

            Assert.Equal(userProducts.Count, returnedProducts.Count());

            foreach(var expectedProduct in userProducts)
            {
                var actualProduct = returnedProducts.FirstOrDefault(p => p.ProductId == expectedProduct.Id);
                Assert.NotNull(actualProduct);
                Assert.NotNull(expectedProduct.SellerId);
                Assert.Equal(expectedProduct.Id, actualProduct.ProductId);
                Assert.Equal(Guid.Parse(expectedProduct.SellerId), actualProduct.SellerId);
                Assert.Equal(expectedProduct.Title, actualProduct.Title);
                Assert.Equal(expectedProduct.Category, actualProduct.Category);
                Assert.Equal(expectedProduct.Price, actualProduct.Price);
                Assert.Equal(expectedProduct.Description, actualProduct.Description);
                Assert.Equal(expectedProduct.Quantity, actualProduct.Quantity);
                Assert.Equal(expectedProduct.SellerName, actualProduct.SellerName);
                Assert.Equal(expectedProduct.Images.Count, actualProduct.ImageUrls.Count);
            } 
        }

        [Fact]
        public async Task GetUserProducts_UserDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var userId = "notAnId";

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(false);

            // Act
            var result = await _controller.GetUserProducts(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetSingleUserProduct_UserAndProductExist_ReturnsOkWithProduct()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var productId = _testProducts[0].Id;
            var actualProduct = _testProducts[0];

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);
            _mockUserProductService.Setup(service => service.FetchSingleUserProduct(userId, productId)).ReturnsAsync(actualProduct);
            

            // Act
            var result = await _controller.GetSingleUserProduct(userId, productId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ProductForResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedProduct = Assert.IsAssignableFrom<ProductForResponseDto>(okResult.Value);


            Assert.NotNull(actualProduct.SellerId);
            Assert.Equal(actualProduct.Id, returnedProduct.ProductId);
            Assert.Equal(Guid.Parse(actualProduct.SellerId), returnedProduct.SellerId);
            Assert.Equal(actualProduct.Title, returnedProduct.Title);
            Assert.Equal(actualProduct.Category, returnedProduct.Category);
            Assert.Equal(actualProduct.Price, returnedProduct.Price);
            Assert.Equal(actualProduct.Description, returnedProduct.Description);
            Assert.Equal(actualProduct.Quantity, returnedProduct.Quantity);
            Assert.Equal(actualProduct.SellerName, returnedProduct.SellerName);
            Assert.Equal(actualProduct.Images.Count, returnedProduct.ImageUrls.Count);
        }

        [Fact]
        public async Task GetSingleUserProduct_UserDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var userId = "notAnId";
            var productId = _testProducts[0].Id;

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(false);

            // Act
            var result = await _controller.GetSingleUserProduct(userId, productId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetSingleUserProduct_ProductDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var productId = Guid.NewGuid();

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);

            // Act
            var result = await _controller.GetSingleUserProduct(userId, productId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task CreateProduct_WhenProductIsValid_ReturnsCreated()
        {
            // Arrange
            var user = _testUsers[0];
            var userId = _testUsers[0].Id;
            var product = _testProducts[0];
            var productDto = _mapper.Map<ProductForCreationDto>(product);
            var expectedResponseDto = _mapper.Map<ProductForResponseDto>(product);

            _mockUserService.Setup(service => service.FetchUserByIdAsync(userId)).ReturnsAsync(user);
            _mockUserProductService.Setup(service => service.CreateProductAsync(It.IsAny<Product>())).ReturnsAsync(product);  

            // Act
            var result = await _controller.CreateProduct(productDto, userId);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);


            Assert.NotNull(createdResult.RouteValues);
            Assert.Equal("GetSingleUserProduct", createdResult.ActionName);
            Assert.Equal(user.Id, createdResult.RouteValues["userId"]);
            Assert.Equal(product.Id, createdResult.RouteValues["productId"]);

            var responseDto = Assert.IsType<ProductForResponseDto>(createdResult.Value);
            Assert.Equal(expectedResponseDto.ProductId, responseDto.ProductId);
            Assert.Equal(expectedResponseDto.Title, responseDto.Title);
            Assert.Equal(expectedResponseDto.Description, responseDto.Description);
            Assert.Equal(expectedResponseDto.Price, responseDto.Price);
            Assert.Equal(expectedResponseDto.Quantity, responseDto.Quantity);
            Assert.Equal(expectedResponseDto.Category, responseDto.Category);
            Assert.Equal(expectedResponseDto.SellerName, responseDto.SellerName);
            Assert.Equal(expectedResponseDto.SellerId, responseDto.SellerId);
            Assert.True(expectedResponseDto.ImageUrls.SequenceEqual(responseDto.ImageUrls));
        }

        [Fact]
        public async Task CreateProduct_WhenInvalidProduct_ReturnsBadRequest()
        {
            // Arrange
            var user = _testUsers[0];
            var userId = _testUsers[0].Id;
            var product = _testProducts[0];
            var productDto = _mapper.Map<ProductForCreationDto>(product);
            productDto.Price = 0;

            _mockUserService.Setup(service => service.FetchUserByIdAsync(userId)).ReturnsAsync(user);
            _controller.ModelState.AddModelError("Price", "Price must be between £0.01 and £100,000.");

            // Act
            var result = await _controller.CreateProduct(productDto, userId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);

            var modelState = badRequestResult.Value as SerializableError;
            Assert.NotNull(modelState);
            Assert.True(modelState.ContainsKey("Price"));

            var priceError = modelState["Price"] as string[];
            Assert.NotNull(priceError);
            Assert.Contains("Price must be between £0.01 and £100,000.", priceError);
        }

        [Fact]
        public async Task CreateProduct_WhenUserDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var userId = "notAnId";
            var product = _testProducts[0];
            var productDto = _mapper.Map<ProductForCreationDto>(product);

            _mockUserService.Setup(service => service.FetchUserByIdAsync(userId)).ReturnsAsync((User)null!);

            // Act
            var result = await _controller.CreateProduct(productDto, userId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.Equal("This User ID does not exist.", notFoundResult?.Value);
        }

        [Fact]  
        public async Task CreateProduct_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var user = _testUsers[0];
            var userId = _testUsers[0].Id;
            var product = _testProducts[0];
            var productDto = _mapper.Map<ProductForCreationDto>(product);

            _mockUserService.Setup(service => service.FetchUserByIdAsync(userId))
                .ReturnsAsync(user);

            _mockUserProductService.Setup(service => service.CreateProductAsync(It.IsAny<Product>())).ReturnsAsync((Product)null!);

            // Act
            var result = await _controller.CreateProduct(productDto, userId);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Failed to create product due to an internal server error.", objectResult?.Value);
        }

        [Fact]
        public async Task DeleteProduct_WhenValidUserAndProduct_ReturnsNoContent()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var productId = _testProducts[0].Id;
            var product = _testProducts[0];

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);
            _mockUserProductService.Setup(service => service.FetchSingleUserProduct(userId, productId)).ReturnsAsync(product);
            _mockUserProductService.Setup(service => service.RemoveProduct(product)).ReturnsAsync(Result.Success());

            // Act
            var result = await _controller.DeleteProduct(userId, productId);

            // Assert
            var deletedResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, deletedResult.StatusCode);
        }

        [Fact]
        public async Task DeleteProduct_WhenInvalidUser_ReturnsNotFound()
        {
            // Arrange
            var userId = "notAnId";
            var productId = _testProducts[0].Id;
            var product = _testProducts[0];

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteProduct(userId, productId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("This User ID does not exist.", notFoundResult.Value);
        }

        [Fact] 
        public async Task DeleteProduct_WhenInvalidProduct_ReturnsNotFound()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var productId = Guid.NewGuid();

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);
            _mockUserProductService.Setup(service => service.FetchSingleUserProduct(userId, productId)).ReturnsAsync((Product)null!);

            // Act
            var result = await _controller.DeleteProduct(userId, productId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("This Product ID does not exist.", notFoundResult.Value);
        }

        [Fact]
        public async Task DeleteProduct_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var productId = _testProducts[0].Id;
            var product = _testProducts[0];

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);
            _mockUserProductService.Setup(service => service.FetchSingleUserProduct(userId, productId)).ReturnsAsync(product);
            _mockUserProductService.Setup(service => service.RemoveProduct(It.IsAny<Product>())).ReturnsAsync(Result.Fail("An internal server error occurred while deleting the product."));

            // Act
            var result = await _controller.DeleteProduct(userId, productId);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.Equal(500, objectResult?.StatusCode);
            Assert.Equal("An internal server error occurred while deleting the product.", objectResult?.Value);
        }

        [Fact]
        public async Task UpdateProduct_WhenValidRequest_ReturnsNoContent()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var productId = _testProducts[0].Id;
            var product = _testProducts[0];

            var updatedTitle = "Oak Table";
            var updatedDescription = "A sturdy table.";
            var updatedPrice = 80.50m;
            var updatedQuantity = 100;

            var patchDocument = new JsonPatchDocument<ProductForUpdateDto>();
            patchDocument.Replace(p => p.Title, updatedTitle);
            patchDocument.Replace(p => p.Description, updatedDescription);
            patchDocument.Replace(p => p.Price, updatedPrice);
            patchDocument.Replace(p => p.Quantity, updatedQuantity);

            var updatedProduct = new Product(updatedTitle, product.Category, updatedPrice, updatedDescription, updatedQuantity);
            updatedProduct.Id = productId;
            updatedProduct.SellerId = userId;
            updatedProduct.SellerName = product.SellerName;
            


            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);
            _mockUserProductService.Setup(service => service.FetchSingleUserProduct(userId, productId)).ReturnsAsync(product);

            _mockUserProductService.Setup(service => service.UpdateProductAsync(It.IsAny<Product>())).Callback<Product>(p =>
            {
                p.Title = updatedTitle;
                p.Description = updatedDescription;
                p.Price = updatedPrice;
                p.Quantity = updatedQuantity;
            })
                .ReturnsAsync(updatedProduct);

            // Act
            var result = await _controller.UpdateProduct(userId, productId, patchDocument);

            // Assert
            var updatedResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, updatedResult.StatusCode);

            Assert.Equal(updatedTitle, updatedProduct.Title);
            Assert.Equal(updatedDescription, updatedProduct.Description);
            Assert.Equal(updatedPrice, updatedProduct.Price);
            Assert.Equal(updatedQuantity, updatedProduct.Quantity);
        }

        [Fact]
        public async Task UpdateProduct_WhenInvalidRequestBody_ReturnsBadRequest()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var productId = _testProducts[0].Id;
            var product = _testProducts[0];

            var patchDocument = new JsonPatchDocument<ProductForUpdateDto>();
            patchDocument.Replace(p => p.Price, 0);

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);
            _mockUserProductService.Setup(service => service.FetchSingleUserProduct(userId, productId)).ReturnsAsync(product);
            _controller.ModelState.AddModelError("Price", "Price must be between £0.01 and £100,000.");

            // Act
            var result = await _controller.UpdateProduct(userId, productId, patchDocument);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);

            var modelState = badRequestResult.Value as SerializableError;
            Assert.NotNull(modelState);
            Assert.True(modelState.ContainsKey("Price"));

            var priceError = modelState["Price"] as string[];
            Assert.NotNull(priceError);
            Assert.Contains("Price must be between £0.01 and £100,000.", priceError);
        }

        [Fact]
        public async Task UpdateProduct_WhenInvalidUser_ReturnsNotFound()
        {
            // Arrange
            var userId = "notAnId";
            var productId = _testProducts[0].Id;
            var product = _testProducts[0];

            var patchDocument = new JsonPatchDocument<ProductForUpdateDto>();
            patchDocument.Replace(p => p.Title, "Oak Table");

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateProduct(userId, productId, patchDocument);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("This User ID does not exist.", notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateProduct_WhenInvalidProduct_ReturnsNotFound()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var productId = Guid.NewGuid();

            var patchDocument = new JsonPatchDocument<ProductForUpdateDto>();
            patchDocument.Replace(p => p.Title, "Oak Table");

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);
            _mockUserProductService.Setup(service => service.FetchSingleUserProduct(userId, productId)).ReturnsAsync((Product)null!);

            // Act
            var result = await _controller.UpdateProduct(userId, productId, patchDocument);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("This Product ID does not exist.", notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateProduct_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var productId = _testProducts[0].Id;
            var product = _testProducts[0];

            var patchDocument = new JsonPatchDocument<ProductForUpdateDto>();
            patchDocument.Replace(p => p.Title, "Oak Table");

            _mockUserProductService.Setup(service => service.CheckUserExists(userId)).ReturnsAsync(true);
            _mockUserProductService.Setup(service => service.FetchSingleUserProduct(userId, productId)).ReturnsAsync(product);
            _mockUserProductService.Setup(service => service.UpdateProductAsync(It.IsAny<Product>())).ReturnsAsync((Product)null!);

            // Act
            var result = await _controller.UpdateProduct(userId, productId, patchDocument);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.Equal(500, objectResult?.StatusCode);
            Assert.Equal("Failed to update product due to an internal server error.", objectResult?.Value);
        }

        [Fact]
        public async Task CheckUserExists_WhenUserExists_ReturnsTrue()
        {
            // Arrange
            var userId = _testUsers[0].Id;

            _mockUserProductRepository.Setup(repo => repo.UserExistsAsync(userId)).ReturnsAsync(true);

            // Act
            var result = await _service.CheckUserExists(userId);

            // Assert 
            Assert.True(result);
        }

        [Fact]  
        public async Task CheckUserExists_WhenUserDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var userId = "notAnId";

            _mockUserProductRepository.Setup(repo => repo.UserExistsAsync(userId)).ReturnsAsync(false);

            // Act
            var result = await _service.CheckUserExists(userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task FetchUserProductsAsync_WhenUserProductsExist_ReturnsFullProductList()
        {
            // Arrange
            var userId = _testUsers[1].Id;
            var expectedProducts = new List<Product> { _testProducts[2], _testProducts[3], _testProducts[4] };

            _mockUserProductRepository.Setup(repo => repo.GetUserProductsAsync(userId)).ReturnsAsync(expectedProducts);

            // Act
            var result = await _service.FetchUserProductsAsync(userId);

            // Assert
            var returnedProducts = Assert.IsAssignableFrom<IEnumerable<Product>>(result);
            Assert.Equal(expectedProducts.Count(), returnedProducts.Count());
            Assert.IsType<List<Product>>(result);

            foreach(var expectedProduct in expectedProducts)
            {
                Assert.Contains(returnedProducts, p =>
                p.Id == expectedProduct.Id &&
                p.SellerId == expectedProduct.SellerId &&
                p.Title == expectedProduct.Title &&
                p.Description == expectedProduct.Description &&
                p.Price == expectedProduct.Price &&
                p.Quantity == expectedProduct.Quantity &&
                p.Category == expectedProduct.Category &&
                p.SellerName == expectedProduct.SellerName &&
                p.Images.SequenceEqual(expectedProduct.Images)
                );
            }
        }

        [Fact]
        public async Task FetchUserProductsAsync_WhenNoUserProducts_ReturnsEmptyProductList()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var expectedProducts = new List<Product>();

            _mockUserProductRepository.Setup(repo => repo.GetUserProductsAsync(userId)).ReturnsAsync(expectedProducts);

            // Act
            var result = await _service.FetchUserProductsAsync(userId);

            // Assert
            var returnedProducts = Assert.IsAssignableFrom<IEnumerable<Product>>(result);
            Assert.Empty(returnedProducts);
            Assert.IsType<List<Product>>(result);
        }

        [Fact]  
        public async Task FetchSingleUserProduct_WhenProductExists_ReturnsProduct()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var productId = _testProducts[0].Id;
            var actualProduct = _testProducts[0];

            _mockUserProductRepository.Setup(repo => repo.GetSingleUserProductAsync(userId, productId)).ReturnsAsync(actualProduct);

            // Act
            var result = await _service.FetchSingleUserProduct(userId, productId);

            // Assert
            var returnedProduct = Assert.IsType<Product>(result);

            Assert.Equal(actualProduct.Id, returnedProduct.Id);
            Assert.Equal(actualProduct.SellerId, returnedProduct.SellerId);
            Assert.Equal(actualProduct.Title, returnedProduct.Title);
            Assert.Equal(actualProduct.Category, returnedProduct.Category);
            Assert.Equal(actualProduct.Price, returnedProduct.Price);
            Assert.Equal(actualProduct.Description, returnedProduct.Description);
            Assert.Equal(actualProduct.Quantity, returnedProduct.Quantity);
            Assert.Equal(actualProduct.SellerName, returnedProduct.SellerName);
            Assert.Equal(actualProduct.Images.Count, returnedProduct.Images.Count);
            Assert.True(actualProduct.Images.SequenceEqual(returnedProduct.Images));
        }

        [Fact]
        public async Task FetchSingleUserProduct_WhenProductDoesNotExist_ReturnsNull()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var productId = Guid.NewGuid();

            _mockUserProductRepository.Setup(repo => repo.GetSingleUserProductAsync(userId, productId)).ReturnsAsync((Product)null!);

            // Act
            var result = await _service.FetchSingleUserProduct(userId, productId);

            // Assert
            Assert.Null(result);
        }

        [Fact]

        public async Task CreateProductAsync_WhenValidRequest_ReturnsNewProduct()
        {
            // Arrange
            var actualProduct = _testProducts[0];

            _mockUserProductRepository.Setup(repo => repo.AddProductAsync(actualProduct)).ReturnsAsync(actualProduct);

            // Act
            var result = await _service.CreateProductAsync(actualProduct);

            // Assert
            var returnedProduct = Assert.IsType<Product>(result);
            Assert.Equal(actualProduct.Id, returnedProduct.Id);
            Assert.Equal(actualProduct.SellerId, returnedProduct.SellerId);
            Assert.Equal(actualProduct.Title, returnedProduct.Title);
            Assert.Equal(actualProduct.Category, returnedProduct.Category);
            Assert.Equal(actualProduct.Price, returnedProduct.Price);
            Assert.Equal(actualProduct.Description, returnedProduct.Description);
            Assert.Equal(actualProduct.Quantity, returnedProduct.Quantity);
            Assert.Equal(actualProduct.SellerName, returnedProduct.SellerName);
            Assert.Equal(actualProduct.Images.Count, returnedProduct.Images.Count);
            Assert.True(actualProduct.Images.SequenceEqual(returnedProduct.Images));
        }

        [Fact]
        public async Task CreateProductAsync_WhenValidRequest_ReturnsCorrectRoundedPrice()
        {
            // Arrange
            var actualProduct = _testProducts[0];
            actualProduct.Price = 6.755M;

            _mockUserProductRepository.Setup(repo => repo.AddProductAsync(actualProduct)).ReturnsAsync(actualProduct);

            // Act
            var result = await _service.CreateProductAsync(actualProduct);

            // Assert
            Assert.Equal(6.76, (double)result.Price, precision: 2);
        }

        [Fact]
        public async Task RemoveProduct_WhenDeletionSucceeds_ReturnsSuccessfulResult()
        {
            // Arrange
            var actualProduct = _testProducts[0];

            _mockUserProductRepository.Setup(repo => repo.DeleteProductAsync(actualProduct)).ReturnsAsync(Result.Success());

            // Act
            var result = await _service.RemoveProduct(actualProduct);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Null(result.Error);
        }

        [Fact]
        public async Task RemoveProduct_WhenDeletionFails_ReturnsFailureResult()
        {
            // Arrange
            var actualProduct = _testProducts[0];

            _mockUserProductRepository.Setup(repo => repo.DeleteProductAsync(actualProduct)).ReturnsAsync(Result.Fail("An internal server error occurred while deleting the product."));

            // Act
            var result = await _service.RemoveProduct(actualProduct);

            // Assert
            Assert.Equal("An internal server error occurred while deleting the product.", result.Error);
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async Task UpdateProductAsync_WhenValidRequest_ReturnsProduct()
        {
            // Arrange
            var actualProduct = _testProducts[0];
            actualProduct.Quantity = 25; // Original quantity was 10.

            _mockUserProductRepository.Setup(repo => repo.UpdateProductAsync(actualProduct)).ReturnsAsync(actualProduct);

            // Act
            var result = await _service.UpdateProductAsync(actualProduct);

            // Assert
            var returnedProduct = Assert.IsType<Product>(result);
            Assert.Equal(25, returnedProduct.Quantity);
        }

        [Fact]
        public async Task UpdateProductAsync_WhenValidRequest_RoundsPriceCorrectly()
        {
            // Arrange
            var actualProduct = _testProducts[0];
            actualProduct.Price = 45.575M; 

            _mockUserProductRepository.Setup(repo => repo.UpdateProductAsync(actualProduct)).ReturnsAsync(actualProduct);

            // Act
            var result = await _service.UpdateProductAsync(actualProduct);

            // Assert
            var returnedProduct = Assert.IsType<Product>(result);
            Assert.Equal(45.58M, returnedProduct.Price);
        }
    }
}
