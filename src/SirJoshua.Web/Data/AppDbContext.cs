using Microsoft.EntityFrameworkCore;
using SirJoshua.Web.Models;

namespace SirJoshua.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Ebook> Ebooks => Set<Ebook>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    // IELTS Band 7 Plus™ funnel: leads, bookings and writing-feedback submissions.
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<WritingFeedbackSubmission> WritingFeedbackSubmissions => Set<WritingFeedbackSubmission>();
    public DbSet<WritingFeedbackPackage> WritingFeedbackPackages => Set<WritingFeedbackPackage>();

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

        b.Entity<Lead>(e =>
        {
            e.HasIndex(l => l.ReferenceNumber);
            e.HasIndex(l => l.CreatedAt);
        });
        b.Entity<WritingFeedbackSubmission>(e => e.HasIndex(s => s.ReferenceNumber));
        b.Entity<WritingFeedbackPackage>(e => e.HasIndex(p => p.ReferenceNumber));

        b.Entity<Ebook>().HasData(SeedData.Ebooks);
    }
}
