using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Controllers;
using ProductService.Data;
using ProductService.Models;

namespace FinalProject.Tests;

public class ProductServiceTest
{

    // Test 1- GetProducts returs all products
    [Fact]
    public async Task GetProducts_ReturnAllProducts()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb1")
            .Options;

        using var context = new AppDbContext(options);
        context.Products.Add(new Product { Id = 1, Name = "Laptop", Price = 1099, ProductQuantity = 10 });
        context.Products.Add(new Product { Id = 2, Name = "Desktop", Price = 900, ProductQuantity = 50 });

        await context.SaveChangesAsync();
        var controller = new ProductController(context);

        // Act
        var result = await controller.GetProducts();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var products = Assert.IsType<List<Product>>(okResult.Value);
        Assert.Equal(2, products.Count);
    }

    // Test 2
    [Fact]
    public async Task GetProduct_ProductId_ReturnProduct()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb2")
            .Options;

        using var context = new AppDbContext(options);
        context.Products.Add(new Product { Id = 1, Name = "Laptop", Price = 1099, ProductQuantity = 10 });
        await context.SaveChangesAsync();

        var controller = new ProductController(context);

        // Act
        var result = await controller.GetProduct(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var product = Assert.IsType<Product>(okResult.Value);
        Assert.Equal(1, product.Id);
        Assert.Equal("Laptop", product.Name);
    }

    // Test 3
    [Fact]
    public async Task CreateProduct_ValidDate_ReturnCreated()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb4")
            .Options;

        using var context = new AppDbContext(options);
        var controller = new ProductController(context);

        var newProduct = new Product
        {
            Name = "Charger",
            Price = 60,
            ProductQuantity = 20,
            ProductCategory = "Electronics"
        };

        // Act
        var result = await controller.CreateProduct(newProduct);

        // Assert
        var createResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var product = Assert.IsType<Product>(createResult.Value);
        Assert.Equal("Charger", product.Name);
    }

    // Test 4:
    [Fact]
    public async Task ProductWorkFlow_CreateProduct_UpdateProduct_DeleteProduct()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "WorkflowTestDb1")
            .Options;

        using var context = new AppDbContext(options);
        var controller = new ProductController(context);

        var newProduct = new Product
        {
            Name = "Gaming Laptop",
            Description = "",
            Price = 2000,
            ProductQuantity = 20,
            ProductCategory = "Electronics"
        };

        var createResult = await controller.CreateProduct(newProduct);
        var createdResult = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        var createdProduct = Assert.IsType<Product>(createdResult.Value);
        int productId = createdProduct.Id;
        
        Assert.Equal("Gaming Laptop", createdProduct.Name);
        Assert.Equal(20, createdProduct.ProductQuantity);

        var stockUpdateResult = await controller.UpdateStock(productId, -5);
        var stockOkResult = Assert.IsType<OkObjectResult>(stockUpdateResult);

        var getResult = await controller.GetProduct(productId);
        var getOkResult = Assert.IsType<OkObjectResult>(getResult.Result);
        var updatedProduct = Assert.IsType<Product>(getOkResult.Value);
        Assert.Equal(15, updatedProduct.ProductQuantity);

        updatedProduct.Price = 1500;
        var updateResult = await controller.UpdateProduct(productId, updatedProduct);
        Assert.IsType<OkObjectResult>(updateResult);

        var finalGetResult = await controller.GetProduct(productId);
        var finalOkResult = Assert.IsType<OkObjectResult>(finalGetResult.Result);
        var finalProduct = Assert.IsType<Product>(finalOkResult.Value);
        Assert.Equal(1500, finalProduct.Price);
        Assert.Equal(15, finalProduct.ProductQuantity);

        var deleteResult = await controller.DeleteProduct(productId);
        Assert.IsType<NoContentResult>(deleteResult);

        var notFoundResult = await controller.GetProduct(productId);
        Assert.IsType<NotFoundObjectResult>(notFoundResult.Result);
    } 
}
