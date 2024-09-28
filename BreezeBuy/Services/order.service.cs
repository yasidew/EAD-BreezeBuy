using BreezeBuy.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;


namespace BreezeBuy.Services
{
    public class OrderService
    {
        private readonly IMongoCollection<Order> _orderCollection;

        public OrderService(IOptions<MongoDbSettings> mongoDbSettings)
        {
            var client = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _orderCollection = database.GetCollection<Order>(mongoDbSettings.Value.OrderCollectionName);
        }

        public async Task PlaceOrderAsync(string customerId, List<CartItem> items, decimal totalAmount, string vendorId)
        {
            var order = new Order
            {
                CustomerId = customerId,
                Items = items,
                TotalAmount = totalAmount,
                Status = OrderStatus.Purchased,
                VendorId = vendorId
            };

            await _orderCollection.InsertOneAsync(order);
        }

        public async Task<List<Order>> GetOrdersByCustomerIdAsync(string customerId)
        {
            return await _orderCollection.Find(order => order.CustomerId == customerId).ToListAsync();
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _orderCollection.Find(order => true).ToListAsync();
        }

        public async Task<List<Order>> GetOrdersByVendorIdAsync(string vendorId)
        {
            return await _orderCollection.Find(order => order.VendorId == vendorId).ToListAsync();
        }

        public async Task MarkOrderAsDeliveredAsync(string orderId)
        {
            var update = Builders<Order>.Update.Set(o => o.Status, OrderStatus.Delivered);
            await _orderCollection.UpdateOneAsync(o => o.Id == orderId, update);
        }
    }
}
