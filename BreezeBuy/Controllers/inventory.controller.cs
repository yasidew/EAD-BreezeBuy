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
        public InventoryController(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        //Get : api/inventory
        [HttpGet]
        public async Task<ActionResult<List<Inventory>>> Get()
        {
            var inventories = await _inventoryService.GetAsync();
            return Ok(inventories);
        }


        //Get : api/inventory/{id}
        [HttpGet("{id:length(24)}", Name = "GetInventory")]
        public async Task<ActionResult<Inventory>> GetById(string id)
        {
            var inventory =  await _inventoryService.GetIByIdAsync(id);
            if(inventory == null)
            {
                return NotFound();
            }
            return Ok(inventory);
        }


        //Post : api/inventory
        [HttpPost]
        public async Task<ActionResult<Inventory>> Create(Inventory newInventory)
        {
            await _inventoryService.CreateAsync(newInventory);
            return CreatedAtRoute("GetInventory", new { id = newInventory.Id.ToString() }, newInventory);
        }

        //Put : api/inventory/{id}
        [HttpPut("{id:length(24)}")]
        public async Task<ActionResult> Update(string id, Inventory updatedInventory)
        {
            var inventory  =  await _inventoryService.GetIByIdAsync(id);
            if(inventory == null)
            {
                return NotFound();
            }
            await _inventoryService.UpdateAsync(id, updatedInventory);
            // return NoContent(); //success
            return Ok( new { message = "Inventory updated successfully" });
        }


        //Delete : api/inventory/{id}
        [HttpDelete("{id:length(24)}")]
        public async Task<ActionResult> Delete(string id)
        {
            var inventory =  await _inventoryService.GetIByIdAsync(id);
            if(inventory == null)
            {
                return NotFound();
            }
            await _inventoryService.RemoveAsync(id);
            // return NoContent(); //success
            return Ok( new { message = "Inventory deleted successfully" });
        }

        //Get low stock items
        [HttpGet("low-stock")]
        public async Task<ActionResult<List<Inventory>>> GetLowstockItems()
        {
            var lowStockItems =  await _inventoryService.GetLowStockItemsAsync();
            return Ok(lowStockItems);
        }
    }
}