using BreezeBuy.Models;
using BreezeBuy.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace BreezeBuy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
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

            // Generate a new Id if it's not provided
            if (string.IsNullOrEmpty(order.Id))
            {
                order.Id = ObjectId.GenerateNewId().ToString();
            }

            try
            {
                await _orderService.CreateOrderAsync(order);
                return CreatedAtRoute("GetOrder", new { id = order.Id }, order);
            }
            catch (InvalidOperationException ex)
            {
                // Handle insufficient stock or other business logic exceptions
                return BadRequest(new { message = ex.Message });
            }

        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> UpdateOrder(string id, Order orderIn)
        {
            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            await _orderService.UpdateOrderAsync(id, orderIn);
            return NoContent();
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
    }
}
