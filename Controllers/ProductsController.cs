using AutoMapper;
using Marketplace.BusinessLayer;
using Marketplace.Models;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Controllers
{
    [Route("/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, IMapper mapper, ILogger<ProductsController> logger)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductForResponseDto>>> GetProducts()
        {
            var products = await _productService.FetchProductsAsync();

            return Ok(_mapper.Map<IEnumerable<ProductForResponseDto>>(products)); 
        }

        [HttpGet("{productId}")]
        public async Task<ActionResult> GetProductById(Guid productId)
        {
            var product = await _productService.FetchProductByIdAsync(productId);

            if (product == null)
            {
                _logger.LogInformation($"Product with ID {productId} wasn't found.");
                return NotFound("This Product ID does not exist.");
            }

            return Ok(_mapper.Map<ProductForResponseDto>(product));
        }
    }
}
