using Marketplace.DataAccess.Entities;

namespace Marketplace.Models
{
    public class OrderForResponseDto
    {
        public Guid OrderId { get; set; }
        public List<OrderItemForResponseDto> OrderItems { get; set; } = new List<OrderItemForResponseDto>();
        public OrderStatus Status { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalPrice { get; set; }
        public string Address {  get; set; } = string.Empty;
        public Guid BuyerId { get; set; }
    }
}
