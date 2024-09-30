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
                Email = model.Email
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
	}



}