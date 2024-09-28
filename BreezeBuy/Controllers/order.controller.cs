using BreezeBuy.Models;
using BreezeBuy.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BreezeBuy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly CartService _cartService;

        public OrderController(OrderService orderService, CartService cartService)
        {
            _orderService = orderService;
            _cartService = cartService;
        }

        // POST: api/order/{customerId}/place
        [HttpPost("{customerId}/place")]
        public async Task<ActionResult> PlaceOrder(string customerId, string vendorId)
        {
            var cart = await _cartService.GetCartByCustomerIdAsync(customerId);
            if (cart == null || cart.Items.Count == 0)
            {
                return BadRequest(new { message = "Cart is empty" });
            }

            var totalAmount = cart.Items.Sum(item => item.Price * item.Quantity);
            await _orderService.PlaceOrderAsync(customerId, cart.Items, totalAmount, vendorId);

            // Clear the cart after placing the order
            await _cartService.ClearCartAsync(customerId);

            return Ok(new { message = "Order placed successfully" });
        }

        // GET: api/order/{customerId}/status
        [HttpGet("{customerId}/status")]
        public async Task<ActionResult<List<Order>>> GetCustomerOrders(string customerId)
        {
            var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId);
            return Ok(orders);
        }

        // GET: api/order/all (Admin/CSR)
        [HttpGet("all")]
        public async Task<ActionResult<List<Order>>> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        // GET: api/order/vendor/{vendorId}
        [HttpGet("vendor/{vendorId}")]
        public async Task<ActionResult<List<Order>>> GetVendorOrders(string vendorId)
        {
            var orders = await _orderService.GetOrdersByVendorIdAsync(vendorId);
            return Ok(orders);
        }

        // PUT: api/order/{orderId}/deliver (Admin/CSR/Vendor)
        [HttpPut("{orderId}/deliver")]
        public async Task<ActionResult> MarkAsDelivered(string orderId)
        {
            await _orderService.MarkOrderAsDeliveredAsync(orderId);
            return Ok(new { message = "Order marked as delivered" });
        }
    }
}
