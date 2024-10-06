using BreezeBuy.Models;
using BreezeBuy.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BreezeBuy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryService _inventoryService;
        private readonly ProductService _productService;
        private readonly OrderService _orderService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(InventoryService inventoryService, ProductService productService, OrderService orderService, ILogger<InventoryController> logger)
        {
            _inventoryService = inventoryService;
            _productService = productService;
            _orderService = orderService;
            _logger = logger;
        }

        // Get : api/inventory
        [HttpGet]
        public async Task<ActionResult<List<Inventory>>> Get()
        {
            var inventories = await _inventoryService.GetAsync();
            return Ok(inventories);
        }

        // Get : api/inventory/{id}
        [HttpGet("{id:length(24)}", Name = "GetInventory")]
        public async Task<ActionResult<Inventory>> GetById(string id)
        {
            var inventory = await _inventoryService.GetIByIdAsync(id);
            if (inventory == null)
            {
                return NotFound(new { message = "Inventory item not found." });
            }
            return Ok(inventory);
        }

        // Get : api/inventory/product/{productId}
        [HttpGet("product/{productId}", Name = "GetInventoryByProductId")]
        public async Task<ActionResult<Inventory>> GetByProductId(string productId)
        {
            var inventory = await _inventoryService.GetByProductIdAsync(productId);
            if (inventory == null)
            {
                return NotFound(new { message = "Inventory item not found." });
            }
            return Ok(inventory);
        }

        // Post : api/inventory
        // [HttpPost]
        // public async Task<ActionResult<Inventory>> Create(Inventory newInventory)
        // {
        //     // Check if the product exists
        //     var product = await _productService.GetProductByIdAsync(newInventory.ItemId);
        //     if (product == null)
        //     {
        //         return BadRequest(new { message = "Product not found." });
        //     }

        //     // Check if the inventory item with the same custom ProductID already exists
        //     var existingInventory = await _inventoryService.GetByProductIdAsync(newInventory.ProductId);
        //     if (existingInventory != null)
        //     {
        //         return BadRequest(new { message = "Inventory item for this custom ProductID already exists." });
        //     }

        //     await _inventoryService.CreateAsync(newInventory);
        //     return CreatedAtRoute("GetInventory", new { id = newInventory.Id.ToString() }, newInventory);
        // }

        [HttpPost]
        public async Task<ActionResult<InventoryResponse>> Create(Inventory newInventory)
        {
            // Check if the product exists
            var product = await _productService.GetProductByIdAsync(newInventory.ItemId);
            if (product == null)
            {
                return BadRequest(new { message = "Product not found." });
            }

            // Check if the inventory item with the same custom ProductID already exists
            var existingInventory = await _inventoryService.GetByProductIdAsync(newInventory.ProductId);
            if (existingInventory != null)
            {
                return BadRequest(new { message = "Inventory item for this custom ProductID already exists." });
            }

            await _inventoryService.CreateAsync(newInventory);

            // Create the response object
            var response = new InventoryResponse
            {
                Id = newInventory.Id,
                ProductId = newInventory.ProductId,
                ProductName = newInventory.ProductName,
                QuantityAvailable = newInventory.QuantityAvailable,
                ReoderLevel = newInventory.ReoderLevel,
                LastUpdated = newInventory.LastUpdated,


                Details = new InventoryDetails
                {
                    ItemId = newInventory.ItemId,
                    Name = newInventory.ProductName,
                }
            };

            return CreatedAtRoute("GetInventory", new { id = newInventory.Id.ToString() }, response);
        }
        // Put : api/inventory/{id}
        [HttpPut("{id:length(24)}")]
        public async Task<ActionResult> Update(string id, Inventory updatedInventory)
        {
            var inventory = await _inventoryService.GetIByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }
            await _inventoryService.UpdateAsync(id, updatedInventory);
            return Ok(new { message = "Inventory updated successfully" });
        }

        // Delete : api/inventory/{id}
        [HttpDelete("{id:length(24)}")]
        public async Task<ActionResult> Delete(string id)
        {
            var inventory = await _inventoryService.GetIByIdAsync(id);
            if (inventory == null)
            {
                return NotFound(new { message = "Inventory item not found or already deleted." });
            }
            await _inventoryService.RemoveAsync(id);
            return Ok(new { message = "Inventory deleted successfully" });
        }

        // Get low stock items
        [HttpGet("low-stock")]
        public async Task<ActionResult<List<Inventory>>> GetLowstockItems()
        {
            var lowStockItems = await _inventoryService.GetLowStockItemsAsync();
            return Ok(lowStockItems);
        }

        [HttpGet("items")]
        public async Task<ActionResult<List<Product>>> GetItems()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }
    }
}