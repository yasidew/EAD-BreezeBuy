// Contains the MongoDbSettings class which is used to store the connection string, database name, and collection names for the MongoDB database.
namespace BreezeBuy.Models
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string InventoryCollectionName { get; set; }
        public string OrderCollectionName { get; set; }
        public string UserCollectionName { get; set; }

		public string VendorCollectionName { get; set; }
        public string ProductCollectionName { get; set; }
        public string CategoryCollectionName { get; set; }
        public string CartCollectionName { get; set; }
    

	}
}