using BreezeBuy.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BreezeBuy.Services
{
    public class InventoryService
    {
        private readonly IMongoCollection<Inventory> _inventoryCollection;

        public InventoryService(IOptions<MongoDbSettings> mongoDbSettings)
        {
            var settings =  mongoDbSettings.Value;
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _inventoryCollection = database.GetCollection<Inventory>(settings.InventoryCollectionName);
        }

        //get all inventory items
        public async Task<List<Inventory>> GetAsync() =>
            await _inventoryCollection.Find(inventory => true).ToListAsync();

        //get inventory item by id
        public async Task<Inventory> GetIByIdAsync(string id) =>
            await _inventoryCollection.Find(inventory => inventory.Id == id).FirstOrDefaultAsync();

        //create a new inventory item
        public async Task CreateAsync(Inventory newInventory) => 
            await _inventoryCollection.InsertOneAsync(newInventory);

        //update an inventory item
        public async Task UpdateAsync(string id, Inventory updatedInventory) =>
            await _inventoryCollection.ReplaceOneAsync(inventory => inventory.Id == id, updatedInventory);

        //delete an inventory item
        public async Task RemoveAsync(string id) =>
            await _inventoryCollection.DeleteOneAsync(inventory => inventory.Id == id);

    }
}