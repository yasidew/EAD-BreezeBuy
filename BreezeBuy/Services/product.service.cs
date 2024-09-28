using BreezeBuy.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BreezeBuy.Services
{
    public class ProductService
    {
        private readonly IMongoCollection<Product> _productCollection;

        public ProductService(IOptions<MongoDbSettings> mongoDbSettings)
        {
            var settings = mongoDbSettings.Value;
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _productCollection = database.GetCollection<Product>(settings.ProductCollectionName);
        }

        // Get all products
        public async Task<List<Product>> GetAllProductsAsync() =>
            await _productCollection.Find(product => true).ToListAsync();

        // Get product by ID
        public async Task<Product> GetProductByIdAsync(string id) =>
            await _productCollection.Find(product => product.Id == id).FirstOrDefaultAsync();

        // Create new product
        public async Task CreateProductAsync(Product newProduct) =>
    await _productCollection.InsertOneAsync(newProduct);

        // Update product
        public async Task UpdateProductAsync(string id, Product updatedProduct)
        {
            updatedProduct.Id = id;
            await _productCollection.ReplaceOneAsync(product => product.Id == id, updatedProduct);
        }

        // Delete product
        public async Task DeleteProductAsync(string id) =>
            await _productCollection.DeleteOneAsync(product => product.Id == id);

        // Activate or deactivate product
        // public async Task SetProductStatusAsync(string id, bool isActive)
        // {
        //     var update = Builders<Product>.Update.Set(product => product.IsActive, isActive);
        //     await _productCollection.UpdateOneAsync(product => product.Id == id, update);
        // }
    }
}
