using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BreezeBuy.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("username")]
        public string Username { get; set; }

		[BsonElement("email")]
		public string Email { get; set; }

		[BsonElement("passwordHash")]
        public string PasswordHash { get; set; }

        [BsonElement("roles")]
        public List<string> Roles { get; set; } = new List<string>();

		[BsonElement("status")]
		public string Status { get; set; } = "active";
	}
}