namespace BreezeBuy.Models
{
    public class CartItem
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }  // Quantity of this product in the cart
        public decimal TotalPrice => Price * Quantity;  // Total price for this cart item
    }

    public class Cart
    {
        public string Id { get; set; }  // Unique Cart ID
        public string UserId { get; set; }  // Customer ID associated with the cart
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public decimal TotalAmount => Items.Sum(item => item.TotalPrice);  // Total amount of the cart
    }
}
