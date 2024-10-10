/*
 * JwtSettings.cs
 * Author: [Dayananda I.H.M.B.L. | IT21307058]
 
 * The JwtSettings class holds the configuration settings 
 * for JWT authentication, including the secret key, issuer, 
 * audience, and token expiration time in minutes.rties.
 */

namespace BreezeBuy.Models
{
    public class JwtSettings
	{
        public string Secretkey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int Expiryinminutes { get; set; }
    }
}