using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class OrderItemForCreationDto
    {
        [Required]
        public Guid ProductId { get; set; }
        [Required]
        [Range(1, 10000, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
        [Required]
        [Range(0.01, 100000.00, ErrorMessage = "Total Price must be between £0.01 and £100,000.")]
        public decimal TotalPrice { get; set; }
        [Required]
        [Range(0.01, 100000.00, ErrorMessage = "Price of each order item must be between £0.01 and £100,000.")]
        public decimal Price { get; set; }
    }
}
