using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;

namespace Marketplace.BusinessLayer
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> FetchOrdersAsync(string userId);
        Task<Order?> FetchOrderByIdAsync(string userId, Guid orderId);
        Task<Order> CreateOrderAsync(Order order);
        Task<Result> RemoveOrder(Order order);
        Task<Order> UpdateOrderAsync(Order order);
        Task<IEnumerable<OrderItem>> FetchSoldItems(string userId);
    }
}
