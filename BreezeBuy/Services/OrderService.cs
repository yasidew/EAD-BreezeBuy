using BreezeBuy.Models;
using BreezeBuy.Repositories;

namespace BreezeBuy.Services
{
    public class OrderService
    {
        private readonly OrderRepository _orderRepository;
        private readonly InventoryService _inventoryService;

        public OrderService(OrderRepository orderRepository, InventoryService inventoryService)
        {
            _orderRepository = orderRepository;
            _inventoryService = inventoryService;
        }

        public async Task<List<Order>> GetOrdersAsync()
        {
            return await _orderRepository.GetOrdersAsync();
        }

        public async Task<Order> GetOrderByIdAsync(string id)
        {
            return await _orderRepository.GetOrderByIdAsync(id);
        }

        // public async Task CreateOrderAsync(Order order)
        // {
        //     await _orderRepository.CreateOrderAsync(order);
        // }

        public async Task CreateOrderAsync(Order order)
        {
            // Check inventory levels before creating the order
            foreach (var item in order.Items)
            {
                var inventoryItem = await _inventoryService.GetByProductIdAsync(item.ProductId);
                if (inventoryItem == null || inventoryItem.QuantityAvailable < item.Quantity)
                {
                    throw new InvalidOperationException("Insufficient stock for product: " + item.ProductId);
                }
            }

            // Create the order
            await _orderRepository.CreateOrderAsync(order);

            // Notify InventoryService to update inventory levels
            await _inventoryService.UpdateInventoryLevelsAsync(order);
        }

        public async Task UpdateOrderAsync(string id, Order order)
        {
            await _orderRepository.UpdateOrderAsync(id, order);
        }

        public async Task DeleteOrderAsync(string id)
        {
            await _orderRepository.DeleteOrderAsync(id);
        }


        // New method to check for pending orders for a product
        public async Task<bool> HasPendingOrdersForProduct(string productId)
        {
            var orders = await _orderRepository.GetOrdersAsync();
            foreach (var order in orders)
            {
                if (order.Items.Any(item => item.ProductId == productId && order.Status == "Pending"))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
