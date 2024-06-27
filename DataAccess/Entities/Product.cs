using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.DataAccess.Entities
{
    public class Product
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        [Required]
        [MaxLength(500)]
        public string Description { get; set; }
        [Required]
        [Range(0.01, 100000.00, ErrorMessage = "Price must be between £0.01 and £100,000.")]
        public decimal Price { get; set; }
        [Required]
        [Range(1, 10000, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
        [Required]
        [MaxLength(100)]
        public string Category { get; set; }
        [Required]
        public string? SellerName { get; set; }
        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

        [ForeignKey("SellerId")]
        public User Seller { get; set; } = null!;
        public string? SellerId { get; set; }

        public Product(string title, string category, decimal price, string description, int quantity)
        {
            Title = title;
            Category = category;
            Price = price;
            Description = description;
            Quantity = quantity;
        }
    }
}
