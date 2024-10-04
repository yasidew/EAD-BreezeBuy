using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace BreezeBuy.Models
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("orderNumber")]
        public string OrderNumber { get; set; }

        [BsonElement("customer")]
        public string Customer { get; set; }

        [BsonElement("items")]
        public List<OrderItem> Items { get; set; }

        // Set default status to "pending"
        [BsonElement("status")]
        public string Status { get; set; } = "pending";

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [BsonElement("totalPayment")]
        public decimal TotalPayment { get; set; }
    }

    public class OrderItem
{
    public string ProductId { get; set; } // The ID of the product
    public int Quantity { get; set; } // Quantity of the product in the order

    [BsonElement("price")]
    public decimal Price { get; set; } // Price of the product, fetched from Product collection

    [BsonElement("totalAmount")]
    public decimal TotalAmount { get; set; } // Total amount = Price * Quantity
}

}
