using WebApplication1.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<OrderService>();

// Add CORS configuration
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", builder =>
	{
		builder.AllowAnyOrigin()  // Allow any origin
			   .AllowAnyMethod()  // Allow any HTTP method
			   .AllowAnyHeader(); // Allow any headers
	});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS globally
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
