using Marketplace.DataAccess.Entities;
using Marketplace.DataAccess.Repositories;
using Marketplace.Helpers;

namespace Marketplace.BusinessLayer
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository) 
        {
           _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }
        
        public async Task<IEnumerable<Order>> FetchOrdersAsync(string userId)
        {
            return await _orderRepository.GetOrdersAsync(userId);
        }

        public async Task<Order?> FetchOrderByIdAsync(string userId, Guid orderId)
        {
            return await _orderRepository.GetOrderByIdAsync(userId, orderId);
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            return await _orderRepository.AddOrderAsync(order);
        }

        public async Task<Result> RemoveOrder(Order order)
        {
            return await _orderRepository.DeleteOrderAsync(order);
        }

        public async Task<Order> UpdateOrderAsync(Order order)
        {
            return await _orderRepository.UpdateOrderAsync(order);
        }
    }
}
