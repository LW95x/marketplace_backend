using AutoMapper;
using Marketplace.BusinessLayer;
using Marketplace.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Marketplace.Controllers
{
    [Route("/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductsController> _logger;
        const int maxProductsPageSize = 30;

        public ProductsController(IProductService productService, IMapper mapper, ILogger<ProductsController> logger)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        /// <summary>
        /// Get all products.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductForResponseDto>>> GetProducts(string? title, string? category, decimal? minPrice, decimal? maxPrice, int pageNumber = 1, int pageSize = 10)
        {
            if (pageSize > maxProductsPageSize)
            {
                pageSize = maxProductsPageSize;
            }

            var (products, paginationMetadata) = await _productService.FetchProductsAsync(title, category, minPrice, maxPrice, pageNumber, pageSize);

            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            return Ok(_mapper.Map<IEnumerable<ProductForResponseDto>>(products)); 
        }
        /// <summary>
        /// Get a specific product.
        /// </summary>
        [HttpGet("{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductForResponseDto>> GetProductById(Guid productId)
        {
            var product = await _productService.FetchProductByIdAsync(productId);

            if (product == null)
            {
                _logger.LogError($"Product with ID {productId} wasn't found.");
                return NotFound("This Product ID does not exist.");
            }

            return Ok(_mapper.Map<ProductForResponseDto>(product));
        }
    }
}
