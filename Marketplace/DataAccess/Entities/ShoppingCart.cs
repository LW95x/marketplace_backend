using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.DataAccess.Entities
{
    public class ShoppingCart
    {
        [Key]
        public Guid Id { get; set; }
        public virtual ICollection<ShoppingCartItem> Items { get; set; } = new List<ShoppingCartItem>();
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; } = 0;
        [ForeignKey("BuyerId")]
        public User User { get; set; } = null!;
        public string? BuyerId { get; set; }
    }
}
