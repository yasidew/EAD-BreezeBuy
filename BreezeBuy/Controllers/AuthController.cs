using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using BreezeBuy.Models;
using BreezeBuy.Data;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
 
namespace BreezeBuy.Controllers
{
 
 
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly JwtSettings _jwtSettings;
 
        public AuthController(MongoDbContext context, IOptions<JwtSettings> jwtSettings)
        {
            _userCollection = context.Users;
            _jwtSettings = jwtSettings.Value;
        }
 
		// Register Api Endpoint
		// "https://localhost:7260/Auth/register"
 
		[HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var user = new User
            {
                Username = model.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Email = model.Email,
				Status = "active"
			};
 
            await _userCollection.InsertOneAsync(user);
            return Ok();
        }
 
		// Login Api Endpoint
		// "https://localhost:7260/Auth/login"
		[HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userCollection.Find(u => u.Username == model.Username).FirstOrDefaultAsync();
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                return Unauthorized();
            }
 
			// Check if the user is deactivated
			if (user.Status == "deactivated")
			{
				return Unauthorized("User is deactivated and cannot log in.");
			}
 
			Console.WriteLine(user.Username);
 
			var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secretkey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                 new Claim(ClaimTypes.Name, user.Username),
				 new Claim(ClaimTypes.NameIdentifier, user.Id),
				 new Claim(ClaimTypes.Role, string.Join(",", user.Roles))
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.Expiryinminutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
 
            return Ok(new { Token = tokenString });
        }
 
		// Current User
		// "https://localhost:7260/Auth/me"
		[HttpGet("me")]
		public async Task<IActionResult> GetLoggedInUser()
		{
			// Extract the user information from the JWT claims
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var username = User.FindFirst(ClaimTypes.Name)?.Value;
 
			if (string.IsNullOrEmpty(userId))
			{
				return BadRequest("UserId not found in token");
			}
 
 
			// Optionally fetch full user details from the database using userId
			var user = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
			if (user == null)
			{
				return NotFound("User not found");
			}
 
			return Ok(new
			{
				UserId = user.Id,
				Username = user.Username,
				Email = user.Email,
				Roles = user.Roles
			});
		}
 
		[Authorize]
		[HttpPut("update")]
		public async Task<IActionResult> UpdateUser([FromBody] UpdateUserModel model)
		{
			// Get the logged-in user's ID from the JWT claims
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
 
			if (string.IsNullOrEmpty(userId))
			{
				return BadRequest("UserId not found in token");
			}
 
			// Fetch the current user from the database
			var user = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
			if (user == null)
			{
				return NotFound("User not found");
			}
 
			// Update the username if provided
			if (!string.IsNullOrEmpty(model.Username))
			{
				user.Username = model.Username;
			}
 
			// Update the email if provided
			if (!string.IsNullOrEmpty(model.Email))
			{
				user.Email = model.Email;
			}
 
			// Handle password change
			if (!string.IsNullOrEmpty(model.NewPassword) && !string.IsNullOrEmpty(model.ConfirmNewPassword))
			{
				if (string.IsNullOrEmpty(model.CurrentPassword))
				{
					return BadRequest("Current password is required to change the password.");
				}
 
				// Verify current password
				if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
				{
					return Unauthorized("Current password is incorrect.");
				}
 
				// Ensure new password and confirmation match
				if (model.NewPassword != model.ConfirmNewPassword)
				{
					return BadRequest("New password and confirmation do not match.");
				}
 
				// Hash and update the new password
				user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
			}
 
			// Update the user record in the database
			await _userCollection.ReplaceOneAsync(u => u.Id == user.Id, user);
 
			return Ok(new
			{
				UserId = user.Id,
				Username = user.Username,
				Email = user.Email,
				Message = "User details updated successfully."
			});
		}
 
		
		[Authorize]
		[HttpPut("deactivateAccount")]
		public async Task<IActionResult> DeactivateAccount()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
 
			if (string.IsNullOrEmpty(userId))
			{
				return BadRequest("UserId not found in token");
			}
 
			// Fetch the user from the database
			var user = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
			if (user == null)
			{
				return NotFound("User not found");
			}
 
			// Check if the user is currently active
			if (user.Status != "active")
			{
				return BadRequest("Only active accounts can be deactivated.");
			}
 
			// Set the status to "deactivated"
			user.Status = "deactivated";
 
			// Update the user in the database
			await _userCollection.ReplaceOneAsync(u => u.Id == user.Id, user);
 
			return Ok(new
			{
				UserId = user.Id,
				Username = user.Username,
				Status = user.Status,
				Message = "Account successfully deactivated."
			});
		}
 
		
 

		[Authorize(Roles = "CSR")]
		[HttpPut("activateCustomerAccount")]
		public async Task<IActionResult> ActivateCustomerAccount([FromBody] ActivateAccountRequest request)
		{
			// Ensure a CSR is performing the action
			var csrUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
 
			if (string.IsNullOrEmpty(csrUserId))
			{
				return BadRequest("CSR UserId not found in token");
			}
 
			if (string.IsNullOrEmpty(request.CustomerId))
			{
				return BadRequest("CustomerId is required");
			}
 
			// Fetch the customer from the database using the provided customerId
			var customer = await _userCollection.Find(u => u.Id == request.CustomerId).FirstOrDefaultAsync();
 
			if (customer == null)
			{
				return NotFound("Customer not found");
			}
 
			// Check if the customer's account is deactivated
			if (customer.Status != "deactivated")
			{
				return BadRequest("Only deactivated accounts can be activated.");
			}
 
			// Set the customer's status to "active"
			customer.Status = "active";
 
			// Update the customer status in the database
			await _userCollection.ReplaceOneAsync(u => u.Id == customer.Id, customer);
 
			return Ok(new
			{
				CustomerId = customer.Id,
				Username = customer.Username,
				Status = customer.Status,
				Message = "Customer account successfully activated by CSR."
			});
		}
 
		[Authorize]
		[HttpPut("activateAccount")]
		public async Task<IActionResult> ActivateAccount()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
 
			if (string.IsNullOrEmpty(userId))
			{
				return BadRequest("UserId not found in token");
			}
 
			// Fetch the user from the database
			var user = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
			if (user == null)
			{
				return NotFound("User not found");
			}
 
			// Check if the user is currently deactivated
			if (user.Status != "deactivated")
			{
				return BadRequest("Only deactivated accounts can be activated.");
			}
 
			// Set the status to "active"
			user.Status = "active";
 
			// Update the user in the database
			await _userCollection.ReplaceOneAsync(u => u.Id == user.Id, user);
 
			return Ok(new
			{
				UserId = user.Id,
				Username = user.Username,
				Status = user.Status,
				Message = "Account successfully activated."
			});
		}
 
 
	}
 
 
}