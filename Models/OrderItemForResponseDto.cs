namespace Marketplace.Models
{
    public class OrderItemForResponseDto
    {
        public Guid OrderItemId {  get; set; }
        public Guid ProductId { get; set; }
        public Guid OrderId {  get; set; }
        public int Quantity {  get; set; }
        public decimal TotalPrice { get; set; }
        public decimal Price {  get; set; }

    }
}
