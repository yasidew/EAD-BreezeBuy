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

        public async Task UpdateProductAsync(string productId, Product updatedProduct)
        {
            // Check if the product exists
            var existingProduct = await GetProductByIdAsync(productId);
            if (existingProduct == null)
            {
                throw new KeyNotFoundException("Product not found.");
            }

            // Check if the category exists
            var category = await _categoryService.GetCategoryByIdAsync(updatedProduct.CategoryId);
            if (category == null)
            {
                throw new KeyNotFoundException("Category not found.");
            }

            // Update the product in the product collection
            await _productCollection.ReplaceOneAsync(product => product.Id == productId, updatedProduct);

            // Update the product in the category's product list
            await _categoryService.UpdateProductInCategoryAsync(updatedProduct.CategoryId, updatedProduct);
        }

        public async Task DeleteProductAsync(string id)
        {
            // Find the product
            var product = await GetProductByIdAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found.");
            }

            // Delete the product from the Product collection
            await _productCollection.DeleteOneAsync(p => p.Id == id);

            // Remove the product from the associated category
            await _categoryService.RemoveProductFromCategoryAsync(product.CategoryId, id);
        }

        // Update the product status (active or inactive)
        public async Task SetProductStatusAsync(string id, bool isActive)
        {
            var product = await GetProductByIdAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found.");
            }

            var update = Builders<Product>.Update.Set(p => p.IsActive, isActive);
            await _productCollection.UpdateOneAsync(p => p.Id == id, update);
        }

        public async Task<List<Product>> SearchProductsByNameAsync(string productName)
        {
            var filter = Builders<Product>.Filter.Regex("Name", new MongoDB.Bson.BsonRegularExpression(productName, "i")); // Case-insensitive search
            return await _productCollection.Find(filter).ToListAsync();
        }


    }
}
