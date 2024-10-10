/*
 * RegisterModel.cs
 * Author: [Dayananda I.H.M.B.L. | IT21307058]
 
 * The RegisterModel class represents the data required for user registration
 */

namespace BreezeBuy.Models
{
    public class RegisterModel
    {
        public string Username { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }
    }

}