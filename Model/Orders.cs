using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace WebApplication1.Model
{
	public class FoodItem
	{
		public string Description { get; set; }
		public int Id { get; set; }
		public string Images { get; set; }
		public string Name { get; set; }
		public int Quantity { get; set; }
		public double Price { get; set; }
	}

	public class Orders
	{
		[BsonId]
		[BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
		public string? Id { get; set; }

		[BsonElement("food")]
		public List<FoodItem> Food { get; set; }

		[BsonElement("orderNumber"), BsonRepresentation(BsonType.Int32)]
		[Required]
		public int OrderNumber { get; set; }

		[BsonElement("tableNumber"), BsonRepresentation(BsonType.Int32)]
		[Required]
		public int TableNumber { get; set; }

		[BsonElement("createdAt"), BsonRepresentation(BsonType.DateTime)]
		[Required]
		public DateTime CreatedAt { get; set; }

		// Delivery Status - Boolean
		[BsonElement("delivered"), BsonRepresentation(BsonType.Boolean)]
		[Required]
		public bool Delivered { get; set; } = false;  // Delivery status
	}
}
