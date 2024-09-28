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
        public string Id { get; set; }

        public string CustomerId { get; set; } // ID of the customer

        public List<CartItem> Items { get; set; } = new List<CartItem>(); // List of ordered items

        public decimal TotalAmount { get; set; }  // Total amount of the order

        public OrderStatus Status { get; set; } = OrderStatus.Pending; // Order status

        public string VendorId { get; set; }  // Vendor who manages the order

        public DateTime OrderDate { get; set; } = DateTime.Now; // Order date
    }
}
