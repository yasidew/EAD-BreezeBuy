/*
 * AuthController.cs
 * Author: [Dayananda I.H.M.B.L. | IT21307058]
 * This controller handles user authentication and account management, 
   including registration, login, account activation, and deactivation 
   for a MongoDB-based application. It also allows for updating user details 
   and supports role-based authorization for certain actions. 
 
 */

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

		// Registers a new user with hashed password and stores it in the database
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

		// Authenticates a user and generates a JWT token if valid credentials are provided
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

		// Returns the details of the currently logged-in user based on the JWT token
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

		// Allows the logged-in user to update their username, email, or password
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
 
			var user = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
			if (user == null)
			{
				return NotFound("User not found");
			}
 
			if (!string.IsNullOrEmpty(model.Username))
			{
				user.Username = model.Username;
			}
 
			if (!string.IsNullOrEmpty(model.Email))
			{
				user.Email = model.Email;
			}
 
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
 
				if (model.NewPassword != model.ConfirmNewPassword)
				{
					return BadRequest("New password and confirmation do not match.");
				}
 
				user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
			}
 
			await _userCollection.ReplaceOneAsync(u => u.Id == user.Id, user);
 
			return Ok(new
			{
				UserId = user.Id,
				Username = user.Username,
				Email = user.Email,
				Message = "User details updated successfully."
			});
		}

		// Deactivates the currently logged-in user's account
		[Authorize]
		[HttpPut("deactivateAccount")]
		public async Task<IActionResult> DeactivateAccount()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
 
			if (string.IsNullOrEmpty(userId))
			{
				return BadRequest("UserId not found in token");
			}
 
			var user = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
			if (user == null)
			{
				return NotFound("User not found");
			}
 
			if (user.Status != "active")
			{
				return BadRequest("Only active accounts can be deactivated.");
			}
 
			user.Status = "deactivated";
 
			await _userCollection.ReplaceOneAsync(u => u.Id == user.Id, user);
 
			return Ok(new
			{
				UserId = user.Id,
				Username = user.Username,
				Status = user.Status,
				Message = "Account successfully deactivated."
			});
		}



		// Allows a CSR to activate a deactivated customer account
		[Authorize(Roles = "CSR")]
		[HttpPut("activateCustomerAccount")]
		public async Task<IActionResult> ActivateCustomerAccount([FromBody] ActivateAccountRequest request)
		{
			var csrUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
 
			if (string.IsNullOrEmpty(csrUserId))
			{
				return BadRequest("CSR UserId not found in token");
			}
 
			if (string.IsNullOrEmpty(request.CustomerId))
			{
				return BadRequest("CustomerId is required");
			}
 
			var customer = await _userCollection.Find(u => u.Id == request.CustomerId).FirstOrDefaultAsync();
 
			if (customer == null)
			{
				return NotFound("Customer not found");
			}
 
			if (customer.Status != "deactivated")
			{
				return BadRequest("Only deactivated accounts can be activated.");
			}
 
			customer.Status = "active";
 
			await _userCollection.ReplaceOneAsync(u => u.Id == customer.Id, customer);
 
			return Ok(new
			{
				CustomerId = customer.Id,
				Username = customer.Username,
				Status = customer.Status,
				Message = "Customer account successfully activated by CSR."
			});
		}

		// Allows the logged-in user to reactivate their own deactivated account
		[Authorize]
		[HttpPut("activateAccount")]
		public async Task<IActionResult> ActivateAccount()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
 
			if (string.IsNullOrEmpty(userId))
			{
				return BadRequest("UserId not found in token");
			}
 
			var user = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
			if (user == null)
			{
				return NotFound("User not found");
			}
 
			if (user.Status != "deactivated")
			{
				return BadRequest("Only deactivated accounts can be activated.");
			}
 
			user.Status = "active";
 
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