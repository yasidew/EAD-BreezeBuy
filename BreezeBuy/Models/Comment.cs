/*
 * Comment.cs
 * Author: [Dayananda I.H.M.B.L. | IT21307058]
 * The Comment class represents a customer feedback comment,
 */

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BreezeBuy.Models
{
	public class Comment
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string? Id { get; set; } = ObjectId.GenerateNewId().ToString();

		[BsonElement("customerId")]
		[BsonRepresentation(BsonType.ObjectId)]
		public string CustomerId { get; set; }

		[BsonElement("rank")]
		public int Rank { get; set; }

		[BsonElement("commentText")]
		public string CommentText { get; set; }

		[BsonElement("isCommentEditable")]
		public bool IsCommentEditable { get; set; } = true;
	}
}
