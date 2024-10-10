/*
 * UpdateUserModel.cs
 * Author: [Dayananda I.H.M.B.L. | IT21307058]
 
 * The UpdateUserModel class is used for updating a user's information
 */


namespace BreezeBuy.Models

{
	public class UpdateUserModel
	{
		public string Username { get; set; }
		public string Email { get; set; }
		public string CurrentPassword { get; set; } // Required to change password
		public string NewPassword { get; set; }
		public string ConfirmNewPassword { get; set; }
	}

}

