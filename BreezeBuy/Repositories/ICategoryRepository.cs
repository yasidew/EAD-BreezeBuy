using BreezeBuy.Models;
using System.Threading.Tasks;

public interface ICategoryRepository
{
    Task<Category> GetCategoryByIdAsync(string id);
    Task UpdateCategoryAsync(Category category);
}
