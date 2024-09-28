using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BreezeBuy.Models
{
    public class Product
    {
        [BsonId] //bson means binary json which is the format that mongodb uses to store data
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Name { get; set; } // Product name

        public string Description { get; set; } // Description of the product

        public decimal Price { get; set; } // Price of the product
        public bool IsActive { get; set; } = true;

        [BsonRepresentation(BsonType.ObjectId)]
        public string? CategoryId { get; set; }// Reference to the category this product belongs to
    }
}
