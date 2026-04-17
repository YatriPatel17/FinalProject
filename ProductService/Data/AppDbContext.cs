using Microsoft.EntityFrameworkCore;
using ProductService.Models;

namespace ProductService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<Product> Products { get; set; }

        // protected override void OnModelCreating(ModelBuilder modelBuilder)
        // {
        //     base.OnModelCreating(modelBuilder);

        //     // SEED DATA
        //     modelBuilder.Entity<Product>().HasData( 
        //         new Product
        //         {
        //             Name = "Gaming Laptop",
        //             Description = "High performance Gamming laptop",
        //             Price = 1300.99m,
        //             ProductQuantity = 10,
        //             ProductCategory = "Electronics",
        //             CreatedAt = DateTime.UtcNow
        //         },

        //         new Product
        //         {
        //             Id = 2,
        //             Name = "Coffe Mug",
        //             Description = "Ceramic coffee mug",
        //             Price = 13.99m,
        //             ProductQuantity = 100,
        //             ProductCategory = "Kitchen",
        //             CreatedAt = DateTime.UtcNow
        //         }
        //     );
        
        // }

    }
}