using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;

namespace Marketplace.DataAccess.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetOrdersAsync(string userId);
        Task<Order?> GetOrderByIdAsync(string userId, Guid orderId);
        Task<Order> AddOrderAsync(Order order);
        Task<Result> DeleteOrderAsync(Order order);
        Task<Order> UpdateOrderAsync(Order order);
    }
}
