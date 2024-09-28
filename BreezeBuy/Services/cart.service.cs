using BreezeBuy.Models;
using MongoDB.Driver;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;


namespace BreezeBuy.Services
{
    public class CartService
    {
        private readonly IMongoCollection<Cart> _cartCollection;

        public CartService(IOptions<MongoDbSettings> mongoDbSettings)
        {
            var client = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _cartCollection = database.GetCollection<Cart>(mongoDbSettings.Value.CartCollectionName);
        }

        // Add item to the cart
        public async Task AddItemToCartAsync(string customerId, CartItem newItem)
        {
            var cart = await _cartCollection.Find(c => c.CustomerId == customerId).FirstOrDefaultAsync();
            if (cart == null)
            {
                cart = new Cart { CustomerId = customerId };
                cart.Items.Add(newItem);
                await _cartCollection.InsertOneAsync(cart);
            }
            else
            {
                cart.Items.Add(newItem);
                await _cartCollection.ReplaceOneAsync(c => c.Id == cart.Id, cart);
            }
        }

        // Get the cart for a customer
        public async Task<Cart> GetCartByCustomerIdAsync(string customerId)
        {
            return await _cartCollection.Find(c => c.CustomerId == customerId).FirstOrDefaultAsync();
        }

        // Clear the cart after purchase
        public async Task ClearCartAsync(string customerId)
        {
            await _cartCollection.DeleteOneAsync(c => c.CustomerId == customerId);
        }
    }
}
