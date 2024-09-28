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
        private readonly CategoryService _categoryService; // Inject CategoryService

        public ProductService(IOptions<MongoDbSettings> mongoDbSettings, CategoryService categoryService)
        {
            var settings = mongoDbSettings.Value;
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _productCollection = database.GetCollection<Product>(settings.ProductCollectionName);
            _categoryService = categoryService; // Properly inject the CategoryService here
        }

        // Get all products
        public async Task<List<Product>> GetAllProductsAsync() =>
            await _productCollection.Find(product => true).ToListAsync();

        // Get product by ID
        public async Task<Product> GetProductByIdAsync(string id) =>
            await _productCollection.Find(product => product.Id == id).FirstOrDefaultAsync();


        // Create a new product and add it to both the Product collection and the Category
        public async Task CreateProductAsync(Product newProduct)
        {
            // Check if the category exists
            var category = await _categoryService.GetCategoryByIdAsync(newProduct.CategoryId);
            if (category == null)
            {
                throw new KeyNotFoundException("Category not found.");
            }

            // Add the product to the Product collection
            await _productCollection.InsertOneAsync(newProduct);

            // Add the product to the Category's product list
            await _categoryService.AddProductToCategoryAsync(newProduct.CategoryId, newProduct);
        }

        // Delete product
        public async Task DeleteProductAsync(string id) =>
            await _productCollection.DeleteOneAsync(product => product.Id == id);

        // Activate or deactivate product
        public async Task SetProductStatusAsync(string id, bool isActive)
        {
            var update = Builders<Product>.Update.Set(product => product.IsActive, isActive);
            await _productCollection.UpdateOneAsync(product => product.Id == id, update);
        }

    }
}
