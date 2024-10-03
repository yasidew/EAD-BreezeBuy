using BreezeBuy.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace BreezeBuy.Repositories
{
    public class OrderRepository
    {
        private readonly IMongoCollection<Order> _orders;

        // Inject IOptions<MongoDbSettings> instead of MongoDbSettings directly
        public OrderRepository(IOptions<MongoDbSettings> mongoDbSettings)
        {
            // Use the Value property to get the actual MongoDbSettings object
            var settings = mongoDbSettings.Value;
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _orders = database.GetCollection<Order>("Orders");
        }

        public async Task<List<Order>> GetOrdersAsync() =>
            await _orders.Find(order => true).ToListAsync();

        public async Task<Order> GetOrderByIdAsync(string id) =>
            await _orders.Find<Order>(order => order.Id == id).FirstOrDefaultAsync();

        public async Task CreateOrderAsync(Order order) =>
            await _orders.InsertOneAsync(order);

        public async Task UpdateOrderAsync(string id, Order order) =>
            await _orders.ReplaceOneAsync(o => o.Id == id, order);

        public async Task DeleteOrderAsync(string id) =>
            await _orders.DeleteOneAsync(o => o.Id == id);
    }
}
