using CheeseIt.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CheeseIt.Api.Data;

public class CheeseItDbContext : DbContext
{
    public CheeseItDbContext(DbContextOptions<CheeseItDbContext> options) : base(options)
    {
    }

    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<CheeseBatch> Batches => Set<CheeseBatch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Recipe>(e =>
        {
            e.Property(r => r.Name).IsRequired().HasMaxLength(100);
            e.Property(r => r.Notes).HasMaxLength(2000);
            e.Property(r => r.Milk).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(r => r.Name).IsUnique();
        });

        modelBuilder.Entity<CheeseBatch>(e =>
        {
            e.Property(b => b.BatchNumber).IsRequired().HasMaxLength(50);
            e.Property(b => b.MilkLiters).HasPrecision(8, 2);
            e.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(b => b.BatchNumber).IsUnique();

            // A recipe with existing batches must not be deleted silently.
            e.HasOne(b => b.Recipe)
                .WithMany(r => r.Batches)
                .HasForeignKey(b => b.RecipeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
