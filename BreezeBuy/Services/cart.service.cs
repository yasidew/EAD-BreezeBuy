using BreezeBuy.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace BreezeBuy.Services
{
    public class CartService
    {
        private readonly IMongoCollection<Cart> _cartCollection;
        private readonly ProductService _productService;

        public CartService(IOptions<MongoDbSettings> mongoDbSettings, ProductService productService)
        {
            var settings = mongoDbSettings.Value;
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _cartCollection = database.GetCollection<Cart>(settings.CartCollectionName);
            _productService = productService;
        }

        // Add multiple products to the cart
        public async Task AddMultipleProductsToCartAsync(string userId, List<CartItem> cartItems)
        {
            // Find the user's cart
            var cart = await _cartCollection.Find(c => c.UserId == userId).FirstOrDefaultAsync();
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
            }

            foreach (var cartItem in cartItems)
            {
                // Get the product by ID
                var product = await _productService.GetProductByIdAsync(cartItem.ProductId);
                if (product == null || !product.IsActive)
                {
                    throw new KeyNotFoundException($"Product with ID {cartItem.ProductId} not found or inactive.");
                }

                // Check if the product has sufficient quantity
                if (product.Quantity < cartItem.Quantity)
                {
                    throw new InvalidOperationException($"Insufficient quantity for product {product.Name}.");
                }

                // Check if the product is already in the cart
                var existingCartItem = cart.Items.Find(i => i.ProductId == cartItem.ProductId);
                if (existingCartItem != null)
                {
                    // Update the quantity
                    existingCartItem.Quantity += cartItem.Quantity;
                }
                else
                {
                    // Add a new cart item
                    cart.Items.Add(new CartItem
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Price = product.Price,
                        Quantity = cartItem.Quantity
                    });
                }

                // Deduct the product quantity from the inventory
                product.Quantity -= cartItem.Quantity;
                await _productService.UpdateProductAsync(product.Id, product);
            }

            // Save the cart
            await _cartCollection.ReplaceOneAsync(c => c.UserId == userId, cart, new ReplaceOptions { IsUpsert = true });
        }
    }
}
