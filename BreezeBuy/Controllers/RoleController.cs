/*
 * RoleController.cs
 * Author: [Dayananda I.H.M.B.L. | IT21307058]
 * The RoleController manages user roles and provides functionality 
   for assigning roles to users, retrieving a paginated list of users, 
   and deleting a customer or vendor by username. 
 
 */

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

		// Assigns a role to a user if the user exists and the role is not already assigned
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

		// Retrieves a paginated list of users along with the total number of pages
		[HttpGet("get-users")]
		public async Task<IActionResult> GetUsers(int page = 1, int pageSize = 5)
		{
			var skip = (page - 1) * pageSize;
			var users = await _userCollection
				.Find(_ => true)
				.Skip(skip)
				.Limit(pageSize)
				.ToListAsync();

			var totalUsers = await _userCollection.CountDocumentsAsync(_ => true);
			var totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);

			return Ok(new
			{
				users,
				totalPages
			});
		}

		// Deletes a customer or vendor by their username if they exist in the collection
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