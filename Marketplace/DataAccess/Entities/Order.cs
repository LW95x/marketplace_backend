using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.DataAccess.Entities
{
    public enum OrderStatus
    {
        Pending,
        Completed,
        Shipped,
        Cancelled
    }
    public class Order
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;
        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        [Required]
        [MaxLength(255)]
        public string Address { get; set; } = string.Empty;
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        [ForeignKey("BuyerId")]
        public User Buyer { get; set; } = null!;
        public string? BuyerId { get; set; }
    }
}
