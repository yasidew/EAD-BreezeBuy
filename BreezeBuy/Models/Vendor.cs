/*
 * Vendor.cs
 * Author: [Dayananda I.H.M.B.L. | IT21307058]
 
 * The Vendor class represents a vendor in the system,
 */

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BreezeBuy.Models
{
	public class Vendor
	{
		
			[BsonId]
			[BsonRepresentation(BsonType.ObjectId)]
			public string? Id { get; set; } // MongoDB's unique identifier

			[BsonElement("name")]
			public string Name { get; set; } // Vendor Name

		    [BsonElement("product")]
		    public string Product { get; set; } // Vendor Name

		    [BsonElement("description")]
			public string Description { get; set; } // Vendor Description

		[BsonElement("userId")]
		[BsonRepresentation(BsonType.ObjectId)] // Reference to User schema's ObjectId
		public string UserId { get; set; } // User ID who owns this vendor

		[BsonElement("averageRating")]
		public double AverageRating { get; set; } = 0;

		[BsonElement("comments")]
		public List<Comment> Comments { get; set; } = new List<Comment>(); // Comments Collection
	}
}
