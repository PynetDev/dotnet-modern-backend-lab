
using Microsoft.EntityFrameworkCore;
using ModernBackendLab.Models;

namespace ModernBackendLab.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);
        modelBuilder.Entity<Product>()
        .Property(p => p.RowVersion)
        .IsRowVersion();
    }
}