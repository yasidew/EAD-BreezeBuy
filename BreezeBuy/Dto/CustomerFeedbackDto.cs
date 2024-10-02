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
