using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Model;
using MongoDB.Bson;
using MongoDB.Driver;

namespace mong.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class OrderController : ControllerBase
	{
		private readonly IMongoCollection<Orders>? _orders;
		public OrderController(OrderService orderService)
		{
			_orders = orderService.Database?.GetCollection<Orders>("order-details");
		}

		[HttpGet("AllOrder")]
		public async Task<IEnumerable<Orders>> Get()
		{
			return await _orders.Find(FilterDefinition<Orders>.Empty).ToListAsync();
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<Orders?>> GetById(string id)
		{
			var filter = Builders<Orders>.Filter.Eq(x => x.Id, id);
			var order = _orders.Find(filter).FirstOrDefault();
			return order is not null ? Ok(order) : NotFound();
		}

		[HttpPost("CreateOrder")]
		public async Task<ActionResult> Create(OrderDto orderDto)
		{
			if (orderDto == null || orderDto.Food == null || !orderDto.Food.Any())
				return BadRequest("Valid order data is required.");

			try
			{
				var lastOrder = await _orders.Find(new BsonDocument())
											 .Sort(Builders<Orders>.Sort.Descending(o => o.OrderNumber))
											 .Limit(1)
											 .FirstOrDefaultAsync();

				var order = new Orders
				{
					Food = orderDto.Food,
					TableNumber = orderDto.TableNumber,
					OrderNumber = lastOrder != null ? lastOrder.OrderNumber + 1 : 1,
					CreatedAt = DateTime.UtcNow,
					Delivered = false
				};

				await _orders.InsertOneAsync(order);

				return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
			}
			catch (Exception ex)
			{
				// Log the error (use a proper logging mechanism here)
				Console.Error.WriteLine($"Error creating order: {ex.Message}");
				return StatusCode(500, "An error occurred while creating the order.");
			}
		}

		[HttpPut("UpdateOrder")]
		public async Task<ActionResult> Update(Orders order)
		{
			try
			{
				// First, rename the 'ItemName' field to 'Description' in all documents
				var renameFilter = Builders<Orders>.Filter.Exists("ItemName");  // Find documents where 'ItemName' exists
				var renameUpdate = Builders<Orders>.Update.Rename("ItemName", "Description");  // Rename 'ItemName' to 'Description'
				await _orders.UpdateManyAsync(renameFilter, renameUpdate);  // Apply the rename operation

				// Now, update the specific order by its ID
				var filter = Builders<Orders>.Filter.Eq(x => x.Id, order.Id);
				await _orders.ReplaceOneAsync(filter, order);

				return Ok();
			}
			catch (Exception ex)
			{
				// Log the error (optional)
				Console.Error.WriteLine($"Error updating order: {ex.Message}");
				return StatusCode(500, "An error occurred while updating the order.");
			}
		}


		[HttpPut("StartDelivery")]
		public async Task<ActionResult> StartDelivery()
		{
			try
			{
				var order = await _orders
				.Find(o => o.Delivered == false)
				.SortBy(o => o.CreatedAt)
				.FirstOrDefaultAsync();

				if (order != null)
				{
					order.Delivered = true;
					await _orders.ReplaceOneAsync(o => o.Id == order.Id, order);
					return Ok(order.TableNumber);
				}
				else
				{
					return Ok($"There are no undelivered orders left.");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error starting the delivery process: {ex}");
				return StatusCode(500, new { message = "Error starting the delivery process." });
			}
		}

		[HttpPut("UpdateOrderStatus/{orderNumber}")]
		public async Task<ActionResult> UpdateStatus(int orderNumber)
		{
			var filter = Builders<Orders>.Filter.Eq(x => x.OrderNumber, orderNumber);
			var update = Builders<Orders>.Update.Set(x => x.Delivered, true);

			var result = await _orders.UpdateOneAsync(filter, update);

			if (result.ModifiedCount > 0)
			{
				return Ok($"Order {orderNumber} delivery status updated to true.");
			}

			return NotFound($"Order with OrderNumber {orderNumber} not found.");
		}

		[HttpGet("TableOrders")]
		public async Task<ActionResult> GetTableOrders()
		{
			try
			{
				// Filter orders where Delivered is false
				var orders = await _orders
					.Find(o => o.Delivered == false)
					.ToListAsync();
				if (orders.Any())
				{
					return Ok(orders.Select(order => new
					{
						order.TableNumber,
						order.Food
					}).ToList());
				}
				else
				{
					return BadRequest("There are no order left undelivered.");
				}
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine($"Error fetching table orders: {ex.Message}");
				return StatusCode(500, "An error occurred while fetching the table orders.");
			}
		}

		[HttpDelete("RemoveOrder/{id}")]
		public async Task<ActionResult> Delete(string id)
		{
			var filter = Builders<Orders>.Filter.Eq(x => x.Id, id);
			await _orders.DeleteOneAsync(filter);
			return Ok();
		}
	}
}
