using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data;

public class UserContext : DbContext
{
    public UserContext(DbContextOptions<UserContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Données initiales pour la démonstration
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Name = "Jean Dupont", Email = "jean.dupont@example.com", CreatedAt = DateTime.UtcNow },
            new User { Id = 2, Name = "Marie Martin", Email = "marie.martin@example.com", CreatedAt = DateTime.UtcNow }
        );
    }
}
