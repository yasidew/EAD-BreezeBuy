using BreezeBuy.Models;
using MongoDB.Driver;
using System.Threading.Tasks;

public class CategoryRepository : ICategoryRepository
{
    private readonly IMongoCollection<Category> _categories;

    public CategoryRepository(IMongoDatabase database)
    {
        _categories = database.GetCollection<Category>("Categories");
    }

    public async Task<Category> GetCategoryByIdAsync(string id)
    {
        return await _categories.Find<Category>(c => c.Id == id).FirstOrDefaultAsync();
    }

    public async Task UpdateCategoryAsync(Category category)
    {
        await _categories.ReplaceOneAsync(c => c.Id == category.Id, category);
    }
}
