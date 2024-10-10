/*
 * MongoDbContext.cs
 * Author: [Dayananda I.H.M.B.L. | IT21307058]
 * This MongoDbContext class sets up a connection to a 
   MongoDB database using configuration settings for the 
   connection string and database name. It exposes two collections, 
   Users and Roles, to interact with the corresponding MongoDB 
   collections in the database.
 
 */

using BreezeBuy.Models;
using MongoDB.Driver;

namespace BreezeBuy.Data
{
	public class MongoDbContext
	{
		private readonly IMongoDatabase _database;

		public MongoDbContext(IConfiguration configuration)
		{
			var client = new MongoClient(configuration["MongoDbSettings:ConnectionString"]);
			_database = client.GetDatabase(configuration["MongoDbSettings:DatabaseName"]);
		}

		public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
		public IMongoCollection<Role> Roles => _database.GetCollection<Role>("Roles");
	}
}
