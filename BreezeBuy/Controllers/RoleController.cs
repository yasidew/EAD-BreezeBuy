using BreezeBuy.Data;
using BreezeBuy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace BreezeBuy.Controllers
{

    [ApiController]
    [Route("[controller]")]
    //[Authorize(Roles = "Admin")]
    public class RoleController : ControllerBase
    {
        private readonly IMongoCollection<User> _userCollection;

        public RoleController(MongoDbContext context)
        {
            _userCollection = context.Users;
        }

		// Assign Role
		// "https://localhost:7260/Role/assign-role"
		[HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleModel model)
        {
            var user = await _userCollection.Find(u => u.Username == model.Username).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound();
            }

            if (!user.Roles.Contains(model.Role))
            {
                user.Roles.Add(model.Role);
                await _userCollection.ReplaceOneAsync(u => u.Id == user.Id, user);
            }

            return Ok();
        }

		[HttpGet("get-users")]
		public async Task<IActionResult> GetUsers()
		{
			var users = await _userCollection.Find(_ => true).ToListAsync();
			return Ok(users);
		}

		[HttpDelete("delete-customer/{username}")]
		public async Task<IActionResult> DeleteCustomer(string username)
		{
			var user = await _userCollection.Find(u => u.Username == username).FirstOrDefaultAsync();
			if (user == null)
			{
				return NotFound(new { Message = "Customer and Vendor not found" });
			}

			await _userCollection.DeleteOneAsync(u => u.Username == username);
			return Ok(new { Message = "Customer deleted successfully" });
		}
	}

}