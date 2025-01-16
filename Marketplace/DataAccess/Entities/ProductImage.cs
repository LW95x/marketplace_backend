using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.DataAccess.Entities
{
    public class ProductImage
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [MaxLength(255)]
        public string Url {  get; set; }

        [ForeignKey("ProductId")] 
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public ProductImage(string url)
        {
            Url = url;
        }
    }
}
