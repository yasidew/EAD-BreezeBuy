using BreezeBuy.Models;
using BreezeBuy.Repositories;

namespace BreezeBuy.Services
{
    public class OrderService
    {
        private readonly OrderRepository _orderRepository;

        public OrderService(OrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<List<Order>> GetOrdersAsync()
        {
            return await _orderRepository.GetOrdersAsync();
        }

        public async Task<Order> GetOrderByIdAsync(string id)
        {
            return await _orderRepository.GetOrderByIdAsync(id);
        }

        public async Task CreateOrderAsync(Order order)
        {
            await _orderRepository.CreateOrderAsync(order);
        }

        public async Task UpdateOrderAsync(string id, Order order)
        {
            await _orderRepository.UpdateOrderAsync(id, order);
        }

        public async Task DeleteOrderAsync(string id)
        {
            await _orderRepository.DeleteOrderAsync(id);
        }
    }
}
