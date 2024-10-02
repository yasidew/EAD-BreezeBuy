﻿using BreezeBuy.Models;
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
