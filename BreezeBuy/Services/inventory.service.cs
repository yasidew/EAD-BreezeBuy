using BreezeBuy.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;

namespace BreezeBuy.Services
{
    public class InventoryService
    {
        private readonly IMongoCollection<Inventory> _inventoryCollection;
        private readonly OrderService _orderService;
        // private readonly Func<OrderService> _orderServiceFactory;

        private readonly ILogger<InventoryService> _logger;

        public InventoryService(IOptions<MongoDbSettings> mongoDbSettings, ILogger<InventoryService> logger)
        {
            var settings = mongoDbSettings.Value;
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _inventoryCollection = database.GetCollection<Inventory>(settings.InventoryCollectionName);
            // _orderService = orderService; 
            // _orderServiceFactory = orderServiceFactory;
            _logger = logger;
        }

        //get all inventory items
        public async Task<List<Inventory>> GetAsync() =>
            await _inventoryCollection.Find(inventory => true).ToListAsync();

        //get inventory item by id
        public async Task<Inventory> GetIByIdAsync(string id) =>
            await _inventoryCollection.Find(inventory => inventory.Id == id).FirstOrDefaultAsync();

        //get inventory item by productId
        public async Task<Inventory> GetByProductIdAsync(string productId) =>
            await _inventoryCollection.Find(inventory => inventory.ProductId == productId).FirstOrDefaultAsync();

        //create a new inventory item
        public async Task CreateAsync(Inventory newInventory) =>
            await _inventoryCollection.InsertOneAsync(newInventory);

        //update an inventory item
        // public async Task UpdateAsync(string id, Inventory updatedInventory)
        // {
        //     updatedInventory.Id = id; // Ensure the Id is set correctly
        //     await _inventoryCollection.ReplaceOneAsync(x => x.Id == id, updatedInventory);
        // }

        // Update an inventory item
        public async Task UpdateAsync(string id, Inventory updatedInventory)
        {
            updatedInventory.Id = id; // Ensure the Id is set correctly
            await _inventoryCollection.ReplaceOneAsync(x => x.Id == id, updatedInventory);

            // Check if the quantity available is below the reorder level
            if (updatedInventory.QuantityAvailable < updatedInventory.ReoderLevel)
            {
                // Send low stock alert email
                await SendLowStockAlertEmail(updatedInventory.ProductName, updatedInventory.ProductId, "ydewmin@gmail.com"); // Replace with actual vendor email
            }
        }

        // delete an inventory item
        public async Task RemoveAsync(string id)
        {
            await _inventoryCollection.DeleteOneAsync(inventory => inventory.Id == id);
        }

        // public async Task RemoveAsync(string id)
        // {
        //     // Fetch the inventory item by ID
        //     var inventory = await GetIByIdAsync(id);
        //     if (inventory == null)
        //     {
        //         throw new KeyNotFoundException("Inventory item not found.");
        //     }

        //     // Check if there are any pending orders for the product
        //     var hasPendingOrders = await _orderService.HasPendingOrdersForProduct(inventory.ProductId);
        //     if (hasPendingOrders)
        //     {
        //         throw new InvalidOperationException("Cannot remove inventory item with pending orders.");
        //     }

        //     // If no pending orders, proceed with removal
        //     await _inventoryCollection.DeleteOneAsync(inventory => inventory.Id == id);
        // }


        // Send low stock alert email
        public async Task SendLowStockAlertEmail(string productName, string productId, string vendorEmail)
        {
            try
            {
                var fromAddress = new MailAddress("dextorfle@gmail.com", "BreezeBuy Notifications");
                var toAddress = new MailAddress(vendorEmail);
                const string fromPassword = "wtrs uzys gejp zsgd"; // Use a secure method to store and retrieve the password
                const string subject = "Low Stock Alert";
                string body = $"Product {productName} (ID: {productId}) is running low on stock. Please reorder.";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com", // e.g., smtp.gmail.com for Gmail
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true // Set to false if you don't want HTML content
                })
                {
                    await smtp.SendMailAsync(message);
                    _logger.LogInformation($"Low stock alert email sent for product {productName} to {vendorEmail}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send low stock alert email for product {productName} to {vendorEmail}: {ex.Message}");
            }
        }

        // Get low stock items
        public async Task<List<Inventory>> GetLowStockItemsAsync()
        {
            var lowStockItems = await _inventoryCollection.Find(inventory => inventory.QuantityAvailable < inventory.ReoderLevel).ToListAsync();

            // Send notifications for low stock
            foreach (var item in lowStockItems)
            {
                // Here, assume vendor email is fetched based on the product/vendor association (you can adjust based on your product model)
                await SendLowStockAlertEmail(item.ProductName, item.ProductId, "ydewmin@gmail.com");
            }

            return lowStockItems;
        }



        // public async Task RemoveAsync(string id)
        // {
        //     // Check if the product has any pending orders
        //     var hasPendingOrders = await _orderService.HasPendingOrdersForProduct(id);
        //     if (hasPendingOrders)
        //     {
        //         throw new InvalidOperationException("Cannot remove product with pending orders.");
        //     }

        //     // If no pending orders, proceed with removal
        //     await _inventoryCollection.DeleteOneAsync(inventory => inventory.Id == id);
        // }

        // public async Task RemoveAsync(string id)
        // {
        //     var inventory = await GetIByIdAsync(id);
        //     if (inventory != null && !inventory.HasPendingOrders)
        //     {
        //         await _inventoryCollection.DeleteOneAsync(inventory => inventory.Id == id);
        //     }
        //     else
        //     {
        //         throw new InvalidOperationException("Cannot remove inventory item with pending orders.");
        //     }
        // }


        // Update inventory levels based on order
        public async Task UpdateInventoryLevelsAsync(Order order)
        {
            if (order.Status != "purchased")
            {
                _logger.LogInformation($"Order with ID: {order.Id} is not in 'purchased' status. Skipping inventory update.");
                return;
            }
            foreach (var item in order.Items)
            {
                _logger.LogInformation($"Processing item with ProductId: {item.ProductId} and Quantity: {item.Quantity}");

                var inventoryItem = await _inventoryCollection.Find(inventory => inventory.ItemId == item.ProductId).FirstOrDefaultAsync();
                if (inventoryItem != null)
                {
                    _logger.LogInformation($"Found inventory item with ProductId: {inventoryItem.ProductId} and QuantityAvailable: {inventoryItem.QuantityAvailable}");

                    inventoryItem.QuantityAvailable -= item.Quantity;
                    inventoryItem.LastUpdated = DateTime.UtcNow;

                    _logger.LogInformation($"Updated QuantityAvailable for ProductId: {inventoryItem.ProductId} to {inventoryItem.QuantityAvailable}");

                    await _inventoryCollection.ReplaceOneAsync(x => x.Id == inventoryItem.Id, inventoryItem);

                    // Check if the quantity available is below the reorder level
                    if (inventoryItem.QuantityAvailable < inventoryItem.ReoderLevel)
                    {
                        // Send low stock alert email
                        _logger.LogInformation($"QuantityAvailable for ProductId: {inventoryItem.ProductId} is below ReorderLevel: {inventoryItem.ReoderLevel}. Sending low stock alert email.");
                        await SendLowStockAlertEmail(inventoryItem.ProductName, inventoryItem.ProductId, "ydewmin@gmail.com"); // Replace with actual vendor email
                    }
                }
                else
                {
                    _logger.LogWarning($"No inventory item found for ProductId: {item.ProductId}");
                }
            }
        }



    }


}