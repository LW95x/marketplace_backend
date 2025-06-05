using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class ProductForUpdateDto
    {
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        [Range(0.01, 100000.00, ErrorMessage = "Price must be between £0.01 and £100,000.")]
        public decimal? Price { get; set; }
        [Range(1, 10000, ErrorMessage = "Quantity must be at least 1.")]
        public int? Quantity {  get; set; }
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;
        [Range(0.01, 100000.00, ErrorMessage = "Price must be between £0.01 and £100,000.")]
        public decimal DeliveryFee { get; set; }
        public Boolean AllowReturns { get; set; }
        [MaxLength(100)]
        public string Condition { get; set; } = string.Empty;
    }
}
