using Marketplace.DataAccess.DbContexts;
using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.DataAccess.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly MarketplaceContext _context;

        public OrderRepository(MarketplaceContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync(string userId)
        {

            return await _context.Orders
                         .Where(o => o.BuyerId == userId)
                         .Include(o => o.OrderItems)
                         .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(string userId, Guid orderId)
        {
            return await _context.Orders
                         .Where(o => o.BuyerId == userId && o.Id == orderId)
                         .Include(o => o.OrderItems)
                         .FirstOrDefaultAsync();
        }

        public async Task<Order> AddOrderAsync(Order order)
        {
            var productIds = order.OrderItems.Select(i => i.ProductId).ToList();
            var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

            foreach (var orderItem in order.OrderItems)
            {
                var product = products.FirstOrDefault(p => p.Id == orderItem.ProductId);

                if (product == null || product.Quantity < orderItem.Quantity)
                {
                    throw new InvalidOperationException("Product could not be found, or there is insufficient quantity of product.");
                }

                product.Quantity -= orderItem.Quantity;
            }

            _context.Orders.Add(order);

            var userCart = await _context.ShoppingCarts.FirstOrDefaultAsync(u => u.BuyerId == order.BuyerId);

            if (userCart != null)
            {
                userCart.Items.Clear();
                userCart.TotalPrice = 0;
            }

            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Result> DeleteOrderAsync(Order order)
        {
            try
            {
                var orderDependencies = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == order.Id);

                if (orderDependencies == null)
                {
                    return Result.Fail("Order could not be found.");
                }

                _context.OrderItems.RemoveRange(orderDependencies.OrderItems);
                _context.Orders.Remove(order);

                await _context.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public async Task<Order> UpdateOrderAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }
    }
}
