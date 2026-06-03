using Microsoft.EntityFrameworkCore;
using SmartStore.DAL.Models;

namespace SmartStore.DAL.Data;

public class SmartStoreContext : DbContext
{
    public SmartStoreContext(DbContextOptions<SmartStoreContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<AI_Log> AI_Logs { get; set; }
    public DbSet<PurchaseRecommendation> PurchaseRecommendations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(p => p.ProductID);
            e.Property(p => p.CurrentQuantity).HasDefaultValue(0);
            e.Property(p => p.MinQuantity).HasDefaultValue(10);
        });

        modelBuilder.Entity<AI_Log>(e =>
        {
            e.HasKey(l => l.LogID);
            e.Property(l => l.TransactionDate).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<PurchaseRecommendation>(e =>
        {
            e.HasKey(r => r.RecommendationID);
            e.Property(r => r.CreatedDate).HasDefaultValueSql("GETDATE()");
            e.Property(r => r.Status).HasDefaultValue("Pending");
            e.HasOne(r => r.Product)
             .WithMany(p => p.PurchaseRecommendations)
             .HasForeignKey(r => r.ProductID)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
