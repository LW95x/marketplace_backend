using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class ProductForCreationDto
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;
        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        [Required]
        [Range(0.01, 100000.00, ErrorMessage = "Price must be between £0.01 and £100,000.")]
        public decimal Price { get; set; }
        [Required]
        [Range(1, 10000, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;
        [Required]
        [Range(0.01, 100000.00, ErrorMessage = "Price must be between £0.01 and £100,000.")]
        public decimal DeliveryFee { get; set; }
        [Required]
        public Boolean AllowReturns { get; set; }
        [Required]
        [MaxLength(100)]
        public string Condition { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}
