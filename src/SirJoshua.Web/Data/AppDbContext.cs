using Microsoft.EntityFrameworkCore;
using SirJoshua.Web.Models;

namespace SirJoshua.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Ebook> Ebooks => Set<Ebook>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Ebook>().Property(e => e.Rating).HasColumnType("numeric(2,1)");

        b.Entity<Order>(e =>
        {
            e.HasIndex(o => o.OrderNumber).IsUnique();
            e.HasIndex(o => o.PayPalOrderId);
            e.Property(o => o.AmountUsd).HasColumnType("numeric(12,2)");
            e.HasMany(o => o.Items)
             .WithOne(i => i.Order!)
             .HasForeignKey(i => i.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Ebook>().HasData(SeedData.Ebooks);
    }
}
