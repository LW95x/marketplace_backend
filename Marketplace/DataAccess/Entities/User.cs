using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Marketplace.DataAccess.Entities
{
    public class User : IdentityUser
    {
        [MaxLength(255)]
        public string ImageUrl { get; set; } = string.Empty;
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ShoppingCart ShoppingCart { get; set; } = new ShoppingCart();
    }
}
