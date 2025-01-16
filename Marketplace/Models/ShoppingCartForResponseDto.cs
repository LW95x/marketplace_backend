using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class ShoppingCartForResponseDto
    {
        public Guid CartId { get; set; }
        public decimal TotalPrice { get; set; }
        public Guid BuyerId { get; set; }
        public List<ShoppingCartItemForResponseDto> Items { get; set; } = new List<ShoppingCartItemForResponseDto>();
    }
}
