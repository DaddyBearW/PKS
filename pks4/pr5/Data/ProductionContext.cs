using Microsoft.EntityFrameworkCore;
using PKS4.Pr5.Models;

namespace PKS4.Pr5.Data;

public class ProductionContext : DbContext
{
    public ProductionContext(DbContextOptions<ProductionContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductionLine> ProductionLines => Set<ProductionLine>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<ProductMaterial> ProductMaterials => Set<ProductMaterial>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductMaterial>()
            .HasKey(item => new { item.ProductId, item.MaterialId });

        modelBuilder.Entity<ProductMaterial>()
            .HasOne(item => item.Product)
            .WithMany(item => item.ProductMaterials)
            .HasForeignKey(item => item.ProductId);

        modelBuilder.Entity<ProductMaterial>()
            .HasOne(item => item.Material)
            .WithMany(item => item.ProductMaterials)
            .HasForeignKey(item => item.MaterialId);

        modelBuilder.Entity<WorkOrder>()
            .HasOne(item => item.Product)
            .WithMany(item => item.WorkOrders)
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WorkOrder>()
            .HasOne(item => item.ProductionLine)
            .WithMany(item => item.WorkOrders)
            .HasForeignKey(item => item.ProductionLineId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Material>()
            .Property(item => item.Quantity)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Material>()
            .Property(item => item.MinimalStock)
            .HasPrecision(10, 2);

        modelBuilder.Entity<ProductMaterial>()
            .Property(item => item.QuantityNeeded)
            .HasPrecision(10, 2);

        modelBuilder.Entity<WorkOrder>()
            .Property(item => item.StartDate)
            .HasColumnType("timestamp without time zone");

        modelBuilder.Entity<WorkOrder>()
            .Property(item => item.EstimatedEndDate)
            .HasColumnType("timestamp without time zone");
    }
}
