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

        // POST: api/product
        [HttpPost]
        public async Task<ActionResult<Product>> Create(Product newProduct)
        {
            try
            {
                // Check if the product with the same name exists
                var existingProduct = await _productService.GetProductByNameAsync(newProduct.Name);

                if (existingProduct != null)
                {
                    // Product exists, update its quantity
                    existingProduct.Quantity += newProduct.Quantity;

                    await _productService.UpdateProductAsync(existingProduct.Id, existingProduct);

                    return Ok(new { message = "Product quantity updated successfully", product = existingProduct });
                }
                else
                {
                    // Product doesn't exist, create a new one
                    await _productService.CreateProductAsync(newProduct);
                    return CreatedAtRoute("GetProduct", new { id = newProduct.Id.ToString() }, newProduct);
                }
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



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



        // PUT: api/product/{id}
        [HttpPut("{id:length(24)}")]
        public async Task<ActionResult> Update(string id, Product updatedProduct)
        {
            if (id != updatedProduct.Id)
            {
                return BadRequest("Product ID mismatch.");
            }

            try
            {
                await _productService.UpdateProductAsync(id, updatedProduct);
                return Ok(new { message = "Product updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // DELETE: api/product/{id}
        [HttpDelete("{id:length(24)}")]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return Ok(new { message = "Product deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET: api/product/search?name={productName}
        [HttpGet("search")]
        public async Task<ActionResult<List<Product>>> SearchByName(string name)
        {
            var products = await _productService.SearchProductsByNameAsync(name);

            if (products == null || products.Count == 0)
            {
                return NotFound(new { message = "No products found with the given name" });
            }

            return Ok(products);
        }


    }
}
