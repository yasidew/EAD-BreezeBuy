/*
 * Role.cs
 * Author: [Dayananda I.H.M.B.L. | IT21307058]
 
 * The Role class represents a role in the system
 */


using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BreezeBuy.Models
{
    public class Role
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }
    }
}