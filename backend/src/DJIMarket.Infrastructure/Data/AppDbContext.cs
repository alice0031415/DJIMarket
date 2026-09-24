using DJIMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DJIMarket.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Manager> Managers => Set<Manager>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Manager>(e =>
        {
            e.ToTable("managers");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.Property(x => x.Team).HasMaxLength(80).IsRequired();
            e.Property(x => x.Position).HasMaxLength(80).IsRequired();
            e.Property(x => x.AvatarSeed).HasMaxLength(80).IsRequired();
            e.HasIndex(x => new { x.IsActive, x.Team });
        });

        modelBuilder.Entity<Customer>(e =>
        {
            e.ToTable("customers");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.Property(x => x.Company).HasMaxLength(160).IsRequired();
            e.Property(x => x.Segment).HasMaxLength(40).IsRequired();
            e.HasIndex(x => x.Segment);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("categories");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("products");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(160).IsRequired();
            e.HasOne(x => x.Category).WithMany(x => x.Products).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.CategoryId, x.IsActive });
        });

        modelBuilder.Entity<Sale>(e =>
        {
            e.ToTable("sales");
            e.HasKey(x => x.Id);
            e.Property(x => x.SoldAt).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            e.HasOne(x => x.Manager).WithMany(x => x.Sales).HasForeignKey(x => x.ManagerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Customer).WithMany(x => x.Sales).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.SoldAt, x.Status });
            e.HasIndex(x => new { x.ManagerId, x.SoldAt, x.Status });
            e.HasIndex(x => x.CustomerId);
        });

        modelBuilder.Entity<SaleItem>(e =>
        {
            e.ToTable("sale_items");
            e.HasKey(x => x.Id);
            e.Property(x => x.Quantity).IsRequired();
            e.Property(x => x.SalePrice).HasPrecision(18, 2).IsRequired();
            e.Property(x => x.CostPrice).HasPrecision(18, 2).IsRequired();
            e.HasOne(x => x.Sale).WithMany(x => x.Items).HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Product).WithMany(x => x.SaleItems).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.SaleId);
            e.HasIndex(x => x.ProductId);
        });
    }
}
