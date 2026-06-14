using MangaShelf.DAL;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.Tests;

// Test-specific DbContext that ignores audit requirements
public class TestMangaDbContext : MangaDbContext
{
    public TestMangaDbContext(DbContextOptions<MangaDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Make audit fields optional for testing
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var createdByProperty = entityType.FindProperty("CreatedBy");
            if (createdByProperty != null)
            {
                createdByProperty.IsNullable = true;
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Set audit fields automatically for testing
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity &&
                        (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            
            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTimeOffset.Now;
                entity.CreatedBy = entity.CreatedBy ?? "TestSystem";
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.UpdatedAt = DateTimeOffset.Now;
                entity.UpdatedBy = "TestSystem";
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}