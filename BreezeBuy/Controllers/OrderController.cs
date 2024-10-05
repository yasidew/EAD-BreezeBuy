using BreezeBuy.Models;
using BreezeBuy.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson;

namespace BreezeBuy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly InventoryService _inventoryService;

        public OrderController(OrderService orderService, InventoryService inventoryService)
        {
            _orderService = orderService;
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Order>>> GetOrders()
        {
            return await _orderService.GetOrdersAsync();
        }

        [HttpGet("{id:length(24)}", Name = "GetOrder")]
        public async Task<ActionResult<Order>> GetOrderById(string id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return order;
        }

        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(Order order)
        {
            if (string.IsNullOrEmpty(order.Id))
            {
                order.Id = ObjectId.GenerateNewId().ToString();
            }

            await _orderService.CreateOrderAsync(order);
            return CreatedAtRoute("GetOrder", new { id = order.Id }, order);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> UpdateOrder(string id, Order orderIn)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound(new { message = "Order not found." });
            }

            orderIn.Id = order.Id; // Ensure the Id is not modified

            try
            {
                await _orderService.UpdateOrderAsync(id, orderIn);
                return Ok(new { message = "Order updated successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                return StatusCode(500, new { message = "An error occurred while updating the order.", error = ex.Message });
            }
        }



        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> DeleteOrder(string id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            await _orderService.DeleteOrderAsync(id);
            return NoContent();
        }

        [HttpPut("purchase/{id:length(24)}")]
        public async Task<IActionResult> PurchaseOrder(string id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            // Set order status to "purchased"
            order.Status = "purchased";
            await _orderService.UpdateOrderAsync(id, order);

            await _inventoryService.UpdateInventoryLevelsAsync(order);

            return NoContent();
        }

        [HttpPut("deliver/{id:length(24)}")]
        public async Task<IActionResult> DeliverOrder(string id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            // Set order status to "delivered"
            order.Status = "delivered";
            await _orderService.UpdateOrderAsync(id, order);

            return NoContent();
        }

    }
}


