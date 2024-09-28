using BreezeBuy.Models;
using BreezeBuy.Services;
using Microsoft.AspNetCore.Mvc;
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

        // POST: api/cart/{customerId}/add
        [HttpPost("{customerId}/add")]
        public async Task<ActionResult> AddToCart(string customerId, CartItem newItem)
        {
            await _cartService.AddItemToCartAsync(customerId, newItem);
            return Ok(new { message = "Item added to cart successfully" });
        }

        // GET: api/cart/{customerId}
        [HttpGet("{customerId}")]
        public async Task<ActionResult<Cart>> GetCart(string customerId)
        {
            var cart = await _cartService.GetCartByCustomerIdAsync(customerId);
            if (cart == null)
            {
                return NotFound(new { message = "Cart not found" });
            }

            return Ok(cart);
        }
    }
}
