using BreezeBuy.Models;
using BreezeBuy.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BreezeBuy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        // POST: api/Cart/AddMultipleItems
        [HttpPost("AddMultipleItems")]
        public async Task<IActionResult> AddMultipleItems([FromBody] AddMultipleItemsRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Add multiple products to the cart
                await _cartService.AddMultipleProductsToCartAsync(request.UserId, request.Items);
                return Ok(new { message = "Products added to cart successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    // Request model for adding multiple items to the cart
    public class AddMultipleItemsRequest
    {
        public string UserId { get; set; }
        public List<CartItem> Items { get; set; }
    }
}
