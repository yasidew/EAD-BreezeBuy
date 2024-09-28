using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace BreezeBuy.Models
{
    public class Cart
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string CustomerId { get; set; }  // ID of the customer

        public List<CartItem> Items { get; set; } = new List<CartItem>(); // List of products in the cart
    }

    public class CartItem
    {
        public string ProductId { get; set; }  // ID of the product
        public string ProductName { get; set; }  // Product name
        public int Quantity { get; set; }  // Quantity of the product
        public decimal Price { get; set; }  // Price of the product
    }
}
