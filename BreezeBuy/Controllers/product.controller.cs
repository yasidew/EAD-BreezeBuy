using BreezeBuy.Models;
using BreezeBuy.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BreezeBuy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductController(ProductService productService)
        {
            _productService = productService;
        }

        // // GET: api/product
        // [HttpGet]
        // public async Task<ActionResult<List<Product>>> Get()
        // {
        //     var products = await _productService.GetAllProductsAsync();
        //     return Ok(products);
        // }

// GET: api/product
[HttpGet]
public async Task<ActionResult<List<Product>>> Get()
{
    var products = await _productService.GetAllProductsAsync();
    
    // Filter to only include active products
    var activeProducts = products.Where(product => product.IsActive).ToList();

    return Ok(activeProducts);
}

        // GET: api/product/{id}
        [HttpGet("{id:length(24)}", Name = "GetProduct")]
        public async Task<ActionResult<Product>> GetById(string id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        // POST: api/product
        [HttpPost]
public async Task<ActionResult<Product>> Create(Product newProduct)
{
    await _productService.CreateProductAsync(newProduct);
    // Return the newly created product and its generated ID
    return CreatedAtRoute("GetProduct", new { id = newProduct.Id }, newProduct);
}


        // PUT: api/product/{id}
        [HttpPut("{id:length(24)}")]
        public async Task<ActionResult> Update(string id, Product updatedProduct)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            await _productService.UpdateProductAsync(id, updatedProduct);
            return Ok(new { message = "Product updated successfully" });
        }

        // DELETE: api/product/{id}
        [HttpDelete("{id:length(24)}")]
        public async Task<ActionResult> Delete(string id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            await _productService.DeleteProductAsync(id);
            return Ok(new { message = "Product deleted successfully" });
        }


        [HttpPut("{id:length(24)}/status")]
public async Task<ActionResult> SetProductStatus(string id, [FromQuery] bool isActive)
{
    var product = await _productService.GetProductByIdAsync(id);
    if (product == null)
    {
        return NotFound();
    }

    await _productService.SetProductStatusAsync(id, isActive);
    return Ok(new { message = $"Product {(isActive ? "activated" : "deactivated")} successfully" });
}

    }
}
