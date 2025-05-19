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
        public virtual ICollection<SavedItem> SavedItems { get; set; } = new List<SavedItem>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    }
}
