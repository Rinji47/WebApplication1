﻿using MongoDB.Driver;

namespace WebApplication1.Data
{
	public class OrderService
	{
		private readonly IConfiguration _configuration;
		private readonly IMongoDatabase? _database;

		public OrderService(IConfiguration configuration)
		{
			_configuration = configuration;

			var connectionString = _configuration.GetConnectionString("dbcs");
			var mongoUrl = MongoUrl.Create(connectionString);
			var mongoClient = new MongoClient(mongoUrl);
			_database = mongoClient.GetDatabase(mongoUrl.DatabaseName);
		}

		public IMongoDatabase? Database => _database;
	}
}
