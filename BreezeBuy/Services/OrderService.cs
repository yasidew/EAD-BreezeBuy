using BreezeBuy.Models;
using BreezeBuy.Repositories;
using System.Threading.Tasks;

namespace BreezeBuy.Services
{
    public class OrderService
    {
        private readonly OrderRepository _orderRepository;
        private readonly ProductService _productService;

        private readonly InventoryService _inventoryService;

        public OrderService(OrderRepository orderRepository, ProductService productService)
        {
            _orderRepository = orderRepository;
            _productService = productService;
        }

        // Fetch all orders
        public async Task<List<Order>> GetOrdersAsync()
        {
            return await _orderRepository.GetOrdersAsync();
        }

        // Fetch order by id
        public async Task<Order> GetOrderByIdAsync(string id)
        {
            return await _orderRepository.GetOrderByIdAsync(id);
        }

        // Create a new order and calculate total prices
        public async Task CreateOrderAsync(Order order)
        {
            decimal totalPayment = 0;

            foreach (var item in order.Items)
            {
                // Fetch product details to calculate total price
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                if (product != null)
                {
                    // Assign price from product to order item
                    item.Price = product.Price;

                    // Calculate total amount for the item
                    item.TotalAmount = item.Quantity * item.Price;

                    // Add item total amount to overall order total payment
                    totalPayment += item.TotalAmount;
                }
                else
                {
                    throw new KeyNotFoundException($"Product with ID {item.ProductId} not found.");
                }
            }

            // Set the total payment for the entire order
            order.TotalPayment = totalPayment;

            // Store the order in the database
            await _orderRepository.CreateOrderAsync(order);

            // Notify InventoryService to update inventory levels
            await _inventoryService.UpdateInventoryLevelsAsync(order);
        }

        // Update existing order and recalculate total prices
        public async Task UpdateOrderAsync(string id, Order order)
        {
            decimal totalPayment = 0;

            foreach (var item in order.Items)
            {
                // Fetch product details again to ensure price consistency
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                if (product != null)
                {
                    // Assign price and recalculate total amount for the item
                    item.Price = product.Price;
                    item.TotalAmount = item.Quantity * item.Price;

                    // Add item total to the overall order payment
                    totalPayment += item.TotalAmount;
                }
            }

            // Update the total payment of the order
            order.TotalPayment = totalPayment;

            // Update the order in the database
            await _orderRepository.UpdateOrderAsync(id, order);
        }

        // Delete an order
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
