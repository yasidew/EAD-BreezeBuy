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
        private readonly ProductService _productService;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(IOptions<MongoDbSettings> mongoDbSettings, ILogger<InventoryService> logger, ProductService productService)
        {
            var settings = mongoDbSettings.Value;
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _inventoryCollection = database.GetCollection<Inventory>(settings.InventoryCollectionName);
            _productService = productService;
            _logger = logger;
        }

        //get all inventory items
        public async Task<List<InventoryResponse>> GetAsync()
        {
            var inventories = await _inventoryCollection.Find(inventory => true).ToListAsync();
            var inventoryResponses = new List<InventoryResponse>();

            foreach (var inventory in inventories)
            {
                var product = await _productService.GetProductByIdAsync(inventory.ItemId);
                if (product != null)
                {
                    var inventoryResponse = new InventoryResponse
                    {
                        Id = inventory.Id,
                        ProductId = inventory.ProductId,
                        ProductName = inventory.ProductName,
                        QuantityAvailable = inventory.QuantityAvailable,
                        ReoderLevel = inventory.ReoderLevel,
                        LastUpdated = inventory.LastUpdated,

                        Details = new InventoryDetails
                        {

                            ItemId = inventory.ItemId,
                            Name = product.Name
                        }
                    };
                    inventoryResponses.Add(inventoryResponse);
                }
            }
            return inventoryResponses;
        }

        //get inventory item by id
        public async Task<InventoryResponse> GetIByIdAsync(string id)
        {
            var inventory = await _inventoryCollection.Find(inventory => inventory.Id == id).FirstOrDefaultAsync();
            if (inventory != null)
            {
                var product = await _productService.GetProductByIdAsync(inventory.ItemId);
                if (product != null)
                {
                    return new InventoryResponse
                    {
                        Id = inventory.Id,
                        ProductId = inventory.ProductId,
                        ProductName = inventory.ProductName,
                        QuantityAvailable = inventory.QuantityAvailable,
                        ReoderLevel = inventory.ReoderLevel,
                        LastUpdated = inventory.LastUpdated,

                        Details = new InventoryDetails
                        {

                            ItemId = inventory.ItemId,
                            Name = product.Name
                        }
                    };
                }
            }
            return null;
        }

        //get inventory item by productId
        public async Task<InventoryResponse> GetByProductIdAsync(string productId)
        {
            var inventory = await _inventoryCollection.Find(inventory => inventory.ProductId == productId).FirstOrDefaultAsync();
            if (inventory != null)
            {
                var product = await _productService.GetProductByIdAsync(inventory.ItemId);
                if (product != null)
                {
                    return new InventoryResponse
                    {
                        Id = inventory.Id,
                        ProductId = inventory.ProductId,
                        ProductName = inventory.ProductName,
                        QuantityAvailable = inventory.QuantityAvailable,
                        ReoderLevel = inventory.ReoderLevel,
                        LastUpdated = inventory.LastUpdated,

                        Details = new InventoryDetails
                        {

                            ItemId = inventory.ItemId,
                            Name = product.Name
                        }
                    };
                }
            }
            return null;
        }

        //create a new inventory item
        public async Task CreateAsync(Inventory newInventory) =>
            await _inventoryCollection.InsertOneAsync(newInventory);

        //update an inventory item
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
                    Host = "smtp.gmail.com",
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
        public async Task<List<InventoryResponse>> GetLowStockItemsAsync()
        {
            var lowStockItems = await _inventoryCollection.Find(inventory => inventory.QuantityAvailable < inventory.ReoderLevel).ToListAsync();
            var lowStockResponses = new List<InventoryResponse>();

            foreach (var inventory in lowStockItems)
            {
                var product = await _productService.GetProductByIdAsync(inventory.ItemId);
                if (product != null)
                {
                    var inventoryResponse = new InventoryResponse
                    {
                        Id = inventory.Id,
                        ProductId = inventory.ProductId,
                        ProductName = inventory.ProductName,
                        QuantityAvailable = inventory.QuantityAvailable,
                        ReoderLevel = inventory.ReoderLevel,
                        LastUpdated = inventory.LastUpdated,

                        Details = new InventoryDetails
                        {

                            ItemId = inventory.ItemId,
                            Name = product.Name
                        }
                    };
                    lowStockResponses.Add(inventoryResponse);

                    // Send low stock alert email
                    await SendLowStockAlertEmail(inventory.ProductName, inventory.ProductId, "ydewmin@gmail.com"); // Replace with actual vendor email
                }
            }
            return lowStockResponses;
        }

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
                    inventoryItem.QuantityAvailable -= item.Quantity;
                    inventoryItem.LastUpdated = DateTime.UtcNow;

                    await _inventoryCollection.ReplaceOneAsync(x => x.Id == inventoryItem.Id, inventoryItem);

                    // Check if the quantity available is below the reorder level
                    if (inventoryItem.QuantityAvailable < inventoryItem.ReoderLevel)
                    {
                        // Send low stock alert email
                        await SendLowStockAlertEmail(inventoryItem.ProductName, inventoryItem.ProductId, "ydewmin@gmail.com"); // Replace with actual vendor email
                    }
                }
                else
                {
                    _logger.LogWarning($"Inventory item with ProductId: {item.ProductId} not found.");
                }
            }
        }
    }
}