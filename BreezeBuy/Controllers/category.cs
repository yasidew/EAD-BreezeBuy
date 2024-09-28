using BreezeBuy.Models;
using BreezeBuy.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BreezeBuy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _categoryService;

        public CategoryController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: api/category
        [HttpGet]
        public async Task<ActionResult<List<Category>>> Get()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        // GET: api/category/{id}
        [HttpGet("{id:length(24)}", Name = "GetCategory")]
        public async Task<ActionResult<Category>> GetById(string id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(category);
        }

        // POST: api/category
        [HttpPost]
        public async Task<ActionResult<Category>> Create(Category newCategory)
        {
            await _categoryService.CreateCategoryAsync(newCategory);
            return CreatedAtRoute("GetCategory", new { id = newCategory.Id.ToString() }, newCategory);
        }

        // POST: api/category/{categoryId}/product
        [HttpPost("{categoryId:length(24)}/product")]
        public async Task<ActionResult> AddProductToCategory(string categoryId, Product newProduct)
        {
            try
            {
                await _categoryService.AddProductToCategoryAsync(categoryId, newProduct);
                return Ok(new { message = "Product added to category successfully" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Category not found" });
            }
        }
    }
}
