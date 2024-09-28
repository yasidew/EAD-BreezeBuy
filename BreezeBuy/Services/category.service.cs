using BreezeBuy.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BreezeBuy.Services
{
    public class CategoryService
    {
        private readonly IMongoCollection<Category> _categoryCollection;

        public CategoryService(IOptions<MongoDbSettings> mongoDbSettings)
        {
            var settings = mongoDbSettings.Value;
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _categoryCollection = database.GetCollection<Category>(settings.CategoryCollectionName);
        }

        // Get all categories
        public async Task<List<Category>> GetAllCategoriesAsync() =>
            await _categoryCollection.Find(category => true).ToListAsync();

        // Get category by ID
        public async Task<Category> GetCategoryByIdAsync(string id) =>
            await _categoryCollection.Find(category => category.Id == id).FirstOrDefaultAsync();

        // Create a new category
        public async Task CreateCategoryAsync(Category newCategory) =>
            await _categoryCollection.InsertOneAsync(newCategory);

        // Add a product to a category
        public async Task AddProductToCategoryAsync(string categoryId, Product product)
        {
            var category = await GetCategoryByIdAsync(categoryId);
            if (category == null)
            {
                throw new KeyNotFoundException("Category not found");
            }

            category.Products.Add(product);
            await _categoryCollection.ReplaceOneAsync(c => c.Id == categoryId, category);
        }

        public async Task UpdateProductInCategoryAsync(string categoryId, Product updatedProduct)
        {
            var category = await GetCategoryByIdAsync(categoryId);
            if (category == null)
            {
                throw new KeyNotFoundException("Category not found.");
            }

            // Find and replace the existing product in the category
            var productIndex = category.Products.FindIndex(p => p.Id == updatedProduct.Id);
            if (productIndex == -1)
            {
                throw new KeyNotFoundException("Product not found in the category.");
            }

            category.Products[productIndex] = updatedProduct;

            // Update the category with the modified product list
            await _categoryCollection.ReplaceOneAsync(c => c.Id == categoryId, category);
        }

    }
}
