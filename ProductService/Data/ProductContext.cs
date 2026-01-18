using Microsoft.EntityFrameworkCore;
using ProductService.Models;

namespace ProductService.Data;

public class ProductContext : DbContext
{
    public ProductContext(DbContextOptions<ProductContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Données initiales pour la démonstration
        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Name = "Laptop Dell XPS 15",
                Description = "Ordinateur portable haute performance",
                Price = 1499.99m,
                Stock = 15,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 2,
                Name = "iPhone 15 Pro",
                Description = "Smartphone dernière génération",
                Price = 1199.99m,
                Stock = 25,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 3,
                Name = "Casque Sony WH-1000XM5",
                Description = "Casque audio à réduction de bruit",
                Price = 349.99m,
                Stock = 50,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
