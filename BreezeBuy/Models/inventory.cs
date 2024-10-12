// Inventory.cs
// This file contains the Inventory model, defining properties of each inventory item, 
// including product ID, quantity, reorder level, and timestamps.
// This model is used for CRUD operations in the inventory service.
// Author: [Yasitha Dewmin | IT21440922]

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
        public string ProductId { get; set; }

        [BsonElement("itemId")]
        public string ItemId { get; set; } // MongoDB's unique identifier for the product

        [BsonElement("productName")]
        public string ProductName { get; set; } // Product Name

        [BsonElement("quantityAvailable")]
        public int QuantityAvailable { get; set; } // Quantity Available

        [BsonElement("reorderLevel")]
        public int ReoderLevel { get; set; } // Reorder Level

        [BsonElement("lastUpdated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow; // Last Updated

        // [BsonElement("hasPendingOrders")]
        // public bool HasPendingOrders { get; set; } = false; // New field
    }

public class InventoryResponse
{
    public string Id { get; set; }
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public int QuantityAvailable { get; set; }
    public int ReoderLevel { get; set; }
    public DateTime LastUpdated { get; set; }
    public InventoryDetails Details { get; set; }
}

public class InventoryDetails
{
     public string ItemId { get; set; }
    public string Name { get; set; }
}
}