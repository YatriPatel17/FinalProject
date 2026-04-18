using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {        
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Product Service API",
        Version = "v1",
        Description = "Product Management Microservice"

    });
});

// Database with SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=product.db"));

// Configure part
builder.WebHost.ConfigureKestrel(options =>
{
   options.ListenAnyIP(8081); 
});

var app = builder.Build();

// Using Migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Service API V1");
    c.RoutePrefix = "swagger"; 
});


app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Health Check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "Productservice",
    version = "9.0",
    migrations = "Applied",
    timestamp = DateTime.UtcNow
}));

app.Run();

