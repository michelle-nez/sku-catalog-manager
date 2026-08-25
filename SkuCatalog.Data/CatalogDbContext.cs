using Microsoft.EntityFrameworkCore;
using SkuCatalog.Data.Models;

namespace SkuCatalog.Data;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Money needs an exact SQL type, or SQL Server picks a float-like default.
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasColumnType("decimal(18,2)");

        // The database refuses duplicate SKUs. The screen cannot talk it out of this.
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Sku)
            .IsUnique();

        // A category that still has products cannot be deleted out from under them.
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Starter categories, so the dropdown is never empty on first run.
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Wall Plates" },
            new Category { Id = 2, Name = "Cables" },
            new Category { Id = 3, Name = "Adapters" },
            new Category { Id = 4, Name = "Accessories" });

        base.OnModelCreating(modelBuilder);
    }
}
