namespace BreezeBuy.Models
{
    public class Product
    {
        public string Id { get; set; } // Unique identifier for the product
        public string Name { get; set; } // Product name
        public string Description { get; set; } // Description of the product
        public decimal Price { get; set; } // Price of the product
        public string VendorId { get; set; } // ID of the vendor selling the product
        public bool IsActive { get; set; } // Whether the product is active or inactive
        public string Category { get; set; } // Product category
        public int Stock { get; set; } // Stock available for the product
        public int ReorderLevel { get; set; } // When to trigger a reorder for the product
    }
}
