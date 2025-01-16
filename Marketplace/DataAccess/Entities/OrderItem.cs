using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.DataAccess.Entities
{
    public class OrderItem
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [Range(1, 10000, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } = 0;
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; } = 0;

        [ForeignKey("OrderId")]
        public Order Order { get; set; } = null!;
        public Guid OrderId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;
        public Guid ProductId { get; set; }

        public OrderItem(int quantity, Guid productId)
        {
            Quantity = quantity;
            ProductId = productId;
        }
    }
}
