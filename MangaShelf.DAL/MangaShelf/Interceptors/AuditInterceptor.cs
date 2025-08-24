using MangaShelf.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MangaShelf.DAL.MangaShelf.Interceptors
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            var context = eventData.Context;
            if (context is MangaDbContext mangaContext)
            {
                foreach (var entry in mangaContext.ChangeTracker.Entries<IEntity>())
                {
                    if (entry.State == EntityState.Added)
                    {
                        entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                        entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                    }
                    else if (entry.State == EntityState.Deleted)
                    {
                        entry.Entity.IsDeleted = true;
                        entry.Entity.DeletedAt = DateTimeOffset.UtcNow;
                        entry.State = EntityState.Modified; // Change state to Modified to avoid actual deletion
                    }
                }
            }
            return base.SavingChanges(eventData, result);
        }
    }
}
