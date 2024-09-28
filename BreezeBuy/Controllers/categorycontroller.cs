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

        // GET: api/category
        // [HttpGet]
        // public async Task<ActionResult<List<Category>>> GetAllCategories()
        // {
        //     var categories = await _categoryService.GetAllCategoriesAsync();
        //     return Ok(categories);
        // }

        [HttpGet]
public async Task<ActionResult<List<Category>>> GetAllCategories()
{
    var activeCategories = await _categoryService.GetAllActiveCategoriesAsync();
    return Ok(activeCategories);
}

        // PUT: api/category/{categoryId}/activate
[HttpPut("{categoryId:length(24)}/activate")]
public async Task<ActionResult> ActivateCategory(string categoryId)
{
    try
    {
        await _categoryService.ActivateCategoryAsync(categoryId);
        return Ok(new { message = "Category activated successfully" });
    }
    catch (KeyNotFoundException)
    {
        return NotFound(new { message = "Category not found" });
    }
}

// PUT: api/category/{categoryId}/deactivate
[HttpPut("{categoryId:length(24)}/deactivate")]
public async Task<ActionResult> DeactivateCategory(string categoryId)
{
    try
    {
        await _categoryService.DeactivateCategoryAsync(categoryId);
        return Ok(new { message = "Category deactivated successfully" });
    }
    catch (KeyNotFoundException)
    {
        return NotFound(new { message = "Category not found" });
    }
}

// GET: api/products/searchByCategory?categoryName=categoryName
[HttpGet("/api/products/searchByCategory")]
public async Task<ActionResult<List<Product>>> SearchProductsByCategoryName([FromQuery] string categoryName)
{
    try
    {
        var products = await _categoryService.SearchProductsByCategoryNameAsync(categoryName);
        if (products == null || products.Count == 0)
        {
            return NotFound(new { message = "No products found in the specified category" });
        }

        return Ok(products);
    }
    catch (KeyNotFoundException)
    {
        return NotFound(new { message = "Category not found" });
    }
}


    }
}
