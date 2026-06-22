using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Globalization;

namespace MangaShelf.DAL.Interceptors;

public class WriteVolumeHistoryInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is MangaDbContext mangaContext)
        {
            AddVolumeHistoryEntries(mangaContext);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var context = eventData.Context;
        if (context is MangaDbContext mangaContext)
        {
            AddVolumeHistoryEntries(mangaContext);
        }
        
        return base.SavingChanges(eventData, result);
    }

    private static void AddVolumeHistoryEntries(MangaDbContext context)
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<Volume>().ToList())
        {
            var releaseDateProperty = entry.Property(nameof(Volume.ReleaseDate));
            var isPreorderProperty = entry.Property(nameof(Volume.IsPreorder));

            if (entry.State == EntityState.Added && isPreorderProperty.CurrentValue is true)
            {
                AddHistoryIfMissing(context, entry.Entity.Id, now, HistoryEventType.PreorderStarted, string.Empty, ConvertToString(isPreorderProperty.CurrentValue));

                continue;
            }

            if (entry.State == EntityState.Added && isPreorderProperty.CurrentValue is false)
            {
                AddHistoryIfMissing(context, entry.Entity.Id, now, HistoryEventType.Released, string.Empty, ConvertToString(isPreorderProperty.CurrentValue));

                continue;
            }

            if (entry.State != EntityState.Modified)
            {
                continue;
            }

            if (isPreorderProperty.IsModified && !Equals(isPreorderProperty.OriginalValue, isPreorderProperty.CurrentValue))
            {
                AddHistoryIfMissing(
                    context,
                    entry.Entity.Id,
                    now,
                    isPreorderProperty.CurrentValue is true
                        ? HistoryEventType.PreorderStarted
                        : HistoryEventType.Released,
                    ConvertToString(isPreorderProperty.OriginalValue),
                    ConvertToString(isPreorderProperty.CurrentValue));

                continue;
            }

            if (releaseDateProperty.IsModified && !Equals(releaseDateProperty.OriginalValue, releaseDateProperty.CurrentValue) && ShouldTrackReleaseDateChange(entry))
            {
                AddHistoryIfMissing(
                    context,
                    entry.Entity.Id,
                    now,
                    HistoryEventType.ReleaseDateChanged,
                    ConvertToString(releaseDateProperty.OriginalValue),
                    ConvertToString(releaseDateProperty.CurrentValue));
            }
        }
    }

    private static void AddHistoryIfMissing(
        MangaDbContext context,
        Guid volumeId,
        DateTimeOffset timestamp,
        HistoryEventType eventType,
        string? oldValue,
        string? newValue)
    {
        var alreadyTracked = context.ChangeTracker.Entries<VolumeHistory>().Any(entry =>
            entry.State != EntityState.Deleted &&
            entry.Entity.VolumeId == volumeId &&
            entry.Entity.EventType == eventType &&
            entry.Entity.OldValue == oldValue &&
            entry.Entity.NewValue == newValue);

        if (alreadyTracked)
        {
            return;
        }

        context.Set<VolumeHistory>().Add(new VolumeHistory
        {
            VolumeId = volumeId,
            Timestamp = timestamp,
            EventType = eventType,
            OldValue = oldValue,
            NewValue = newValue
        });
    }

    private static bool ShouldTrackReleaseDateChange(EntityEntry<Volume> entry)
    {
        var isPreorderProperty = entry.Property(nameof(Volume.IsPreorder));

        return isPreorderProperty.CurrentValue is true && !isPreorderProperty.IsModified;
    }

    private static string ConvertToString(object? value)
    {
        if (value is null)
        {
            return string.Empty;
        }

        return value switch
        {
            DateTimeOffset dto => dto.ToString("O", CultureInfo.InvariantCulture),
            DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
            Enum enumValue => enumValue.ToString(),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
        };
    }
}
