using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.DataAccess.Entities
{
    public class SavedItem
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;
        public Guid ProductId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
        public string UserId { get; set; } = null!;
    }
}
