using BreezeBuy.Models;
using BreezeBuy.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson;
using System.Security.Claims;

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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found.");
            }

            // Assign the userId to the order's customerId
            order.CustomerId = userId;

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
                return NotFound(new { message = "Order not found." });
            }

            try
            {
                await _orderService.DeleteOrderAsync(id);
                return Ok(new { message = "Order deleted successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                return StatusCode(500, new { message = "An error occurred while deleting the order.", error = ex.Message });
            }
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

         [HttpGet("customer/{customerId:length(24)}")]
        public async Task<ActionResult<List<Order>>> GetOrdersByCustomerId(string customerId)
        {
            var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId);

            if (orders == null || orders.Count == 0)
            {
                return NotFound(new { message = "No orders found for the given customer ID." });
            }

            return Ok(orders);
        }
    }
}


