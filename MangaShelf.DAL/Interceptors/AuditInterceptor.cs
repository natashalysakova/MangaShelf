using MangaShelf.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MangaShelf.DAL.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        AuditEntity(eventData);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AuditEntity(eventData);
        return base.SavingChanges(eventData, result);
    }

    private static void AuditEntity(DbContextEventData eventData)
    {
        var context = eventData.Context;
        if (context is MangaDbContext mangaContext)
        {
            foreach (var entry in mangaContext.ChangeTracker.Entries<IAuditableEntity>())
            {
                if (string.IsNullOrEmpty(entry.Entity.CreatedBy))
                {
                    entry.Entity.CreatedBy = "system";
                }

                if(string.IsNullOrEmpty(entry.Entity.UpdatedBy))
                {
                    entry.Entity.UpdatedBy = "system";
                }

                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                    entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }

            foreach (var entry in mangaContext.ChangeTracker.Entries<IDeletableEntity>())
            {
                if (entry.State == EntityState.Deleted)
                {
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTimeOffset.UtcNow;
                    entry.State = EntityState.Modified; // Change state to Modified to avoid actual deletion
                }
            }
        }
    }
}
