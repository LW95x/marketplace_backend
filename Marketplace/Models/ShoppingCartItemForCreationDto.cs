using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public class ShoppingCartItemForCreationDto
    {
     [Required]
     [Range(1, 10000, ErrorMessage = "Quantity must be at least 1.")]
     public int Quantity { get; set; }
     [Required]
     public Guid ProductId { get; set; }
    }        
}
