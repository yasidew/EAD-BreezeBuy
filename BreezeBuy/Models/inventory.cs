// This file contains the model for the Inventory. It contains the properties of the Inventory.
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BreezeBuy.Models
{
    public class Inventory
    {
        [BsonId] //bson means binary json which is the format that mongodb uses to store data
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } // MongoDB's unique identifier

        [BsonElement("productId")]
        public String ProductId { get; set; } // MongoDB's unique identifier

        [BsonElement("productName")]
        public string ProductName { get; set; } // Product Name

        [BsonElement("quantityAvailable")]
        public int QuantityAvailable{get; set;} // Quantity Available

        [BsonElement("reorderLevel")]
        public int ReoderLevel { get; set; } // Reorder Level

        [BsonElement("lastUpdated")]
        public DateTime LastUpdated{get; set;} = DateTime.UtcNow; // Last Updated
    }
}