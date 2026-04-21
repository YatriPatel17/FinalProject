using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OrderService.Controllers;
using OrderService.Data;
using OrderService.Models;

namespace FinalProject.Tests;

public class OrderServiceTest
{

     private IConfiguration CreateMockConfiguration()
    {
        var configValues = new Dictionary<string, string>
        {
            { "ProductServiceUrl", "http://localhost:8081/" }
        };
        
        return new ConfigurationBuilder()
            .AddInMemoryCollection(configValues!)
            .Build();
    }

    // Test 1:
    [Fact]
    public async Task GetOrder_ReturnAllOrders()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "OrderTestDb1")
            .Options;

        using var context = new AppDbContext(options);

        var orderItem = new OrderItem
        {
            Id = 1,
            ProductId = 1,
            ProductName = "Laptop",
            Quantity = 2,
            UnitPrice = 999,
            OrderId = 1
        };

        var order = new Order
        {
            Id = 1,
            CustomerName = "Yatri Patel",
            CustomerEmail = "test@example.com",
            TotalAmount = 150,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem> { orderItem }
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var controller = new OrderController(context,  CreateMockConfiguration());

        //Act
        var result = await controller.GetOrders();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var orders = Assert.IsType<List<Order>>(okResult.Value);

        Assert.Single(orders);
        Assert.Equal("Yatri Patel", orders.First().CustomerName);
        Assert.Single(orders.First().Items);
    }

    // Test 2: 
    [Fact]
    public async Task CompleteOrderWorkflow_CreateOrder_UpdateStatus_MultiStep()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "WorkflowTestDb")
            .Options;

        using var context = new AppDbContext(options);

        var order = new Order
        {
            Id = 1,
            CustomerName = "Kunj Lakhani",
            CustomerEmail = "kunj@gmail.com",
            TotalAmount = 600,
            Status = "Confirmed",
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = 1,
                    ProductId = 1,
                    ProductName = "Laptop",
                    Quantity = 2,
                    UnitPrice = 250,
                    OrderId = 1
                }
            }
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var controller = new OrderController(context, CreateMockConfiguration());

        var confirmResult = await controller.UpdateOrderStatus(1, new UpdateStatusRequest { Status = "Confirmed" });
        Assert.IsType<OkObjectResult>(confirmResult);

        var updateResult = await controller.UpdateOrderStatus(1, new UpdateStatusRequest { Status = "Shipped" });
        Assert.IsType<OkObjectResult>(updateResult);

        var getResult = await controller.GetOrder(1);
        var okResult = Assert.IsType<OkObjectResult>(getResult.Result);
        var updatedOrder = Assert.IsType<Order>(okResult.Value);
        Assert.Equal("Shipped", updatedOrder.Status);
        Assert.Equal("Kunj Lakhani", updatedOrder.CustomerName);
        Assert.Single(updatedOrder.Items);
    }

    // Test 3
    [Fact]
    public async Task MultipleOrders_ProcessWorkflow()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "MultipleOrderTestDb")
            .Options;

        using var context = new AppDbContext(options);

        var orders = new List<Order>
        {
            new Order
            {
                Id = 1,
                CustomerName = "Kunj",
                CustomerEmail = "kunj@gmail.com",
                TotalAmount = 200,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>()
            },
            new Order
            {
                Id = 2,
                CustomerName = "Yatri",
                CustomerEmail = "yatri@gmail.com",
                TotalAmount = 300,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>()
            },
            new Order
            {
                Id = 3,
                CustomerName = "Archi",
                CustomerEmail = "archi@gmail.com",
                TotalAmount = 400,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>()
            }
        };

        context.Orders.AddRange(orders);
        await context.SaveChangesAsync();

        var controller = new OrderController(context, CreateMockConfiguration());

        var allOrdersResult = await controller.GetOrders();
        var okAllResult = Assert.IsType<OkObjectResult>(allOrdersResult.Result);
        var allOrders = Assert.IsType<List<Order>>(okAllResult.Value);
        Assert.Equal(3, allOrders.Count);

        var pendingOrders = allOrders.Where(o => o.Status == "Pending").ToList();
        Assert.Equal(3, pendingOrders.Count);

        foreach (var pendingOrder in pendingOrders)
        {
            var updateResult = await controller.UpdateOrderStatus(pendingOrder.Id, new UpdateStatusRequest { Status = "Confirmed" });
            Assert.IsType<OkObjectResult>(updateResult);
        }

        var updatedOrdersResult = await controller.GetOrders();
        var okUpdatedResult = Assert.IsType<OkObjectResult>(updatedOrdersResult.Result);
        var updatedOrders = Assert.IsType<List<Order>>(okUpdatedResult.Value);

        foreach (var order in updatedOrders)
        {
            Assert.Equal("Confirmed", order.Status);
        }

        var customerOrdersResult = await controller.GetOrdersByCustomer("kunj@gmail.com");
        var okCustomerResult = Assert.IsType<OkObjectResult>(customerOrdersResult.Result);
        var customerOrders = Assert.IsType<List<Order>>(okCustomerResult.Value);
        Assert.Single(customerOrders);
        Assert.Equal("Kunj", customerOrders.First().CustomerName);
    
    }
}