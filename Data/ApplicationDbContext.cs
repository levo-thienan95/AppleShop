using AppleStore.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AppleStore.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>(e =>
        {
            e.HasIndex(c => c.Slug).IsUnique();
            e.Property(c => c.Name).IsRequired().HasMaxLength(100);
        });

        builder.Entity<Product>(e =>
        {
            e.HasIndex(p => p.Slug).IsUnique();
            e.Property(p => p.Name).IsRequired().HasMaxLength(200);
            e.Property(p => p.Price).HasColumnType("decimal(18,0)");
            e.Property(p => p.OldPrice).HasColumnType("decimal(18,0)");

            e.HasOne(p => p.Category)
             .WithMany(c => c.Products)
             .HasForeignKey(p => p.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Order>(e =>
        {
            e.Property(o => o.Total).HasColumnType("decimal(18,0)");
            e.Property(o => o.Status).HasConversion<string>().HasMaxLength(30);

            e.HasOne(o => o.User)
             .WithMany()
             .HasForeignKey(o => o.UserId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<OrderDetail>(e =>
        {
            e.Property(d => d.UnitPrice).HasColumnType("decimal(18,0)");

            e.HasOne(d => d.Order)
             .WithMany(o => o.OrderDetails)
             .HasForeignKey(d => d.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(d => d.Product)
             .WithMany()
             .HasForeignKey(d => d.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
