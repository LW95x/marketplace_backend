using AutoMapper;
using Marketplace.BusinessLayer;
using Marketplace.Controllers;
using Marketplace.DataAccess.Entities;
using Marketplace.DataAccess.Repositories;
using Marketplace.Helpers;
using Marketplace.MapperProfiles;
using Marketplace.Models;
using Marketplace.Test.Fixtures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace Marketplace.Test
{
    public class ProductTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<ILogger<ProductsController>> _mockLogger;
        private readonly List<User> _testUsers;
        private readonly List<Product> _testProducts;
        private readonly ProductsController _controller;
        private readonly ProductService _service;

        public ProductTests() 
        {
            var config = new MapperConfiguration(c => c.AddProfile<ProductProfile>());
            _mapper = config.CreateMapper();

            _mockProductRepository = new Mock<IProductRepository>();
            _mockProductService = new Mock<IProductService>();
            _mockLogger = new Mock<ILogger<ProductsController>>();

            _testUsers = TestDataFactory.GetUsers();
            _testProducts = TestDataFactory.GetProducts(_testUsers);

            _controller = new ProductsController(_mockProductService.Object, _mapper,  _mockLogger.Object);
            _service = new ProductService(_mockProductRepository.Object);

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };
        }

        [Fact]
        public async Task GetProducts_WhenValidRequest_ReturnsOkWithAllProducts()
        {
            // Arrange
            var products = _testProducts;
            int itemCount = products.Count;
            int pageSize = 10;
            int currentPage = 1;

            var paginationMetadata = new PaginationMetadata(itemCount, pageSize, currentPage);

            // Passing in null properties to test method without additional parameters
            _mockProductService.Setup(service => service.FetchProductsAsync(null, null, null, null, 1, 10)).ReturnsAsync((products, paginationMetadata));

            // Act - Passing in null properties to test method without additional pagination or searching queries
            var result = await _controller.GetProducts(null, null, null, null, 1, 10);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<ProductForResponseDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedProducts = Assert.IsAssignableFrom<IEnumerable<ProductForResponseDto>>(okResult.Value);

            Assert.Equal(products.Count, returnedProducts.Count());
        }

        [Fact]
        public async Task GetProducts_WhenValidRequest_ReturnsMetadataInHeader()
        {
            // Arrange
            var products = _testProducts;
            int itemCount = products.Count;
            int pageSize = 10;
            int currentPage = 1;

            var paginationMetadata = new PaginationMetadata(itemCount, pageSize, currentPage);

            // Passing in null properties to test method without additional parameters
            _mockProductService.Setup(service => service.FetchProductsAsync(null, null, null, null, 1, 10)).ReturnsAsync((products, paginationMetadata));

            // Act - Passing in null properties to test method without additional pagination or searching queries
            var result = await _controller.GetProducts(null, null, null, null, 1, 10);

            // Assert
            var headers = _controller.Response.Headers;
            Assert.True(headers.ContainsKey("X-Pagination"));

            var headerValues = headers["X-Pagination"].ToString();
            var deserializedMetadata = JsonSerializer.Deserialize<PaginationMetadata>(headerValues);

            Assert.NotNull(deserializedMetadata);
            Assert.Equal(itemCount, deserializedMetadata.TotalItemCount);
            Assert.Equal((int)Math.Ceiling(itemCount / (double)pageSize), deserializedMetadata.TotalPageCount);
            Assert.Equal(pageSize, deserializedMetadata.PageSize);
            Assert.Equal(currentPage, deserializedMetadata.CurrentPage);
        }

        [Fact]
        public async Task GetProducts_WhenPageSizeExceedsMaxPageSize_PageSizeIsCapped()
        {
            // Arrange
            var products = _testProducts;
            int itemCount = products.Count;
            int pageSize = 35; // Max page size is defined as 30.
            int cappedPageSize = 30;
            int currentPage = 1;

            var paginationMetadata = new PaginationMetadata(itemCount, cappedPageSize, currentPage);

            // Passing in null properties to test method without additional parameters
            _mockProductService.Setup(service => service.FetchProductsAsync(null, null, null, null, currentPage, cappedPageSize)).ReturnsAsync((products, paginationMetadata));

            // Act
            var result = await _controller.GetProducts(null, null, null, null, currentPage, pageSize);

            // Assert
            var headers = _controller.Response.Headers;
            Assert.True(headers.ContainsKey("X-Pagination"));

            var headerValues = headers["X-Pagination"].ToString();
            var deserializedMetadata = JsonSerializer.Deserialize<PaginationMetadata>(headerValues);

            Assert.NotNull(deserializedMetadata);
            Assert.Equal(cappedPageSize, deserializedMetadata.PageSize);
        }

        [Fact]
        public async Task GetProducts_WhenProductsEmpty_ReturnsEmptyList()
        {
            // Arrange
            var emptyproductsList = new List<Product>();
            int itemCount = emptyproductsList.Count;
            int pageSize = 10;
            int currentPage = 1;

            var paginationMetadata = new PaginationMetadata(itemCount, pageSize, currentPage);

            _mockProductService.Setup(service => service.FetchProductsAsync(null, null, null, null, 1, 10)).ReturnsAsync((emptyproductsList, paginationMetadata));

            // Act
            var result = await _controller.GetProducts(null, null, null, null, 1, 10);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<ProductForResponseDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedProducts = Assert.IsAssignableFrom<IEnumerable<ProductForResponseDto>>(okResult.Value);

            Assert.Equal(emptyproductsList.Count, returnedProducts.Count());
            Assert.Empty(returnedProducts);
        }

        [Fact]
        public async Task GetProducts_WhenFiltersApplied_ReturnsFilteredProducts()
        {
            // Arrange
            var products = _testProducts;
            int itemCount = products.Count;
            int pageSize = 10;
            int currentPage = 1;

            var paginationMetadata = new PaginationMetadata(itemCount, pageSize, currentPage);

            _mockProductService.Setup(service => service.FetchProductsAsync("Table", "Furniture", 50, 110, 1, 10)).ReturnsAsync((products.Where(
                                p => p.Title.Contains("Table") &&
                                p.Category == "Furniture" &&
                                p.Price >= 50 && p.Price <= 110
                                ).Take(10), 
                                paginationMetadata
                                ));

            // Act
            var result = await _controller.GetProducts("Table", "Furniture", 50, 110, 1, 10);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<ProductForResponseDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedProducts = Assert.IsAssignableFrom<IEnumerable<ProductForResponseDto>>(okResult.Value);

            // Products[5] refers to the specific product that should be returned.
            Assert.Equal(products[5].Id, returnedProducts.First().ProductId);
            Assert.Contains("Table", returnedProducts.First().Title);
            Assert.Equal("Furniture", returnedProducts.First().Category);
            Assert.InRange(returnedProducts.First().Price ?? 0M, 50M, 110M);
        }

        [Fact]
        public async Task GetProductById_WhenValidId_ReturnsOkWithSingleProduct()
        {
            // Arrange
            var product = _testProducts[0];

            _mockProductService.Setup(service => service.FetchProductByIdAsync(product.Id)).ReturnsAsync(product);

            // Act
            var result = await _controller.GetProductById(product.Id);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ProductForResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedProduct = Assert.IsAssignableFrom<ProductForResponseDto>(okResult.Value);

            Assert.True(returnedProduct.ProductId == product.Id);
        }

        [Fact]  
        public async Task GetProductById_WhenNotValidId_ReturnsNotFound() 
        {
            // Arrange
            var productId = Guid.NewGuid();

            _mockProductService.Setup(service => service.FetchProductByIdAsync(productId)).ReturnsAsync((Product)null!);


            // Act
            var result = await _controller.GetProductById(productId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task FetchProductsAsync_IfValidRequest_ReturnsCorrectMetadataInTuple()
        {
            // Arrange
            var products = _testProducts;
            int itemCount = products.Count;
            int pageSize = 10;
            int currentPage = 1;

            var paginationMetadata = new PaginationMetadata(itemCount, pageSize, currentPage);

            _mockProductRepository.Setup(repo => repo.GetProductsAsync("Table", "Furniture", 50, 110, 1, 10)).ReturnsAsync((products.Where(
                                p => p.Title.Contains("Table") &&
                                p.Category == "Furniture" &&
                                p.Price >= 50 && p.Price <= 110
                                ).Take(10),
                                paginationMetadata
                                ));


            // Act
            var result = await _service.FetchProductsAsync("Table", "Furniture", 50, 110, 1, 10);

            // Assert
            var (returnedProducts, returnedMetadata) = Assert.IsType<(IEnumerable<Product>, PaginationMetadata)>(result);


            // Products[5] refers to the specific product that should be returned from this filtering request.
            Assert.Equal(products[5].Id, returnedProducts.First().Id);

            Assert.Equal(currentPage, returnedMetadata.CurrentPage);
            Assert.Equal(pageSize, returnedMetadata.PageSize);
            Assert.Equal(itemCount, returnedMetadata.TotalItemCount);
        }

        [Fact]
        public async Task FetchProductByIdAsync_IfValidRequest_ReturnsCorrectProduct()
        {
            // Arrange
            var product = _testProducts[0];

            _mockProductRepository.Setup(repo => repo.GetProductByIdAsync(product.Id)).ReturnsAsync(product);

            // Act
            var result = await _service.FetchProductByIdAsync(product.Id);

            // Assert
            var returnedProduct = Assert.IsType<Product>(result);

            Assert.Equal(product.Id, returnedProduct.Id);
            Assert.Equal(product.SellerId, returnedProduct.SellerId);
        }

        [Fact]
        public async Task FetchProductByIdAsync_IfInvalidProductId_ReturnsNull()
        {
            // Arrange
            var productId = Guid.NewGuid();

            _mockProductRepository.Setup(repo => repo.GetProductByIdAsync(productId)).ReturnsAsync((Product)null!);

            // Act
            var result = await _service.FetchProductByIdAsync(productId);

            // Assert
            Assert.Null(result);
        }

    }
}
