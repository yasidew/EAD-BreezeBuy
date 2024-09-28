using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace BreezeBuy.Models
{
    public class Category
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } // Unique Category ID

        public string Name { get; set; } // Category Name

        public List<Product> Products { get; set; } = new List<Product>(); // List of products in the category

        public bool IsActive { get; set; } = true;
    }
}
