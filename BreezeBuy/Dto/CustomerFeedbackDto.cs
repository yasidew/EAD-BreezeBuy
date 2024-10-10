/*
 * CustomerFeedbackDto.cs
 * Author: [Dayananda I.H.M.B.L. | IT21307058]
 * The CustomerFeedbackDto class is a data transfer object (DTO) that holds information about customer feedback
 
 */

namespace BreezeBuy.Dto
{
	public class CustomerFeedbackDto
	{
		public string VendorId { get; set; }
		public string VendorName { get; set; }
		public string VendorProduct { get; set; }
		public string CommentId { get; set; }
		public string CommentText { get; set; }
		public int Rank { get; set; }
		public bool IsCommentEditable { get; set; }
	}
}
