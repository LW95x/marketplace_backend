namespace Marketplace.Models
{
    public class ShoppingCartItemForResponseDto
    {
        public Guid CartItemId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal Price { get; set; }
        public Guid ProductId { get; set; }
    }
}
