using MangaShelf.Common.Helpers;
using MangaShelf.DAL;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.Migration.DataCorrections;

public class Correction_20260625_002_NormalizeIsbn : IDataCorrection
{
    private readonly ILogger<Correction_20260625_002_NormalizeIsbn> _logger;

    public Correction_20260625_002_NormalizeIsbn(ILogger<Correction_20260625_002_NormalizeIsbn> logger)
    {
        _logger = logger;
    }

    public async Task ApplyCorrection(MangaDbContext context)
    {
        var volumes = await context.Volumes.ToListAsync();
            
        foreach (var volume in volumes)
        {
            if(volume.ISBN != VolumeHelper.NormalizedIsbn(volume.ISBN))
            {
                volume.ISBN = VolumeHelper.NormalizedIsbn(volume.ISBN);
                _logger.LogInformation($"Normalized ISBN for volume '{volume.Id}':'{volume.Title}' to '{volume.ISBN}'");
            }
        }

        await context.SaveChangesAsync();
    }
}

public class Correction_20260625_003_RemoveDuplicatedVolumes : IDataCorrection
{
    private readonly ILogger<Correction_20260625_003_RemoveDuplicatedVolumes> _logger;
    public Correction_20260625_003_RemoveDuplicatedVolumes(ILogger<Correction_20260625_003_RemoveDuplicatedVolumes> logger)
    {
        _logger = logger;
    }

    public async Task ApplyCorrection(MangaDbContext context)
    {
        var volumes = await context.Volumes
            .IgnoreQueryFilters()
            .GroupBy(v => v.ISBN)
            .ToListAsync();

        foreach (var group in volumes)
        {
            if (group.Count() <= 1)
            {
                continue;
            }

            if(group.Key == null)
            {
                continue;
            }

            var volumesToRemove = group.OrderBy(x=>x.CreatedAt).Skip(1).ToList();
            context.Volumes.RemoveRange(volumesToRemove);
            _logger.LogInformation($"Removed {volumesToRemove.Count} duplicated volumes with ISBN '{group.Key}'");
        }

        await context.SaveChangesAsync();

        var toDelete = await context.Volumes
            .IgnoreQueryFilters()
            .Where(x => x.Number != null)  // Add this filter
            .GroupBy(x => new { x.SeriesId, x.Number })
            .ToListAsync();

        foreach (var group in toDelete)
        {
            if (group.Count() <= 1)
            {
                continue;
            }

            var toRemove = group.OrderBy(x => x.CreatedAt).Skip(1).ToList();
            context.Volumes.RemoveRange(toRemove);
            _logger.LogInformation($"Removed {toRemove.Count} duplicated volumes with SeriesId '{group.Key.SeriesId}' and Number '{group.Key.Number}'");
        }

        await context.SaveChangesAsync();

        var oneshotsWitjManyVolumes = await context.Volumes
            .Include(x => x.Series)
            .IgnoreQueryFilters()
            .Where(x => x.Series.Status == DAL.Models.SeriesStatus.OneShot)
            .GroupBy(x => x.SeriesId)
            .ToListAsync();

        foreach (var group in oneshotsWitjManyVolumes)
        {
            if (group.Count() <= 1)
            {
                continue;
            }

            var toRemove = group.OrderBy(x => x.CreatedAt).Skip(1).ToList();
            context.Volumes.RemoveRange(toRemove);
            _logger.LogInformation($"Removed {toRemove.Count} duplicated volumes with SeriesId '{group.Key}'");
        }

        var groupedByUrl = await context.Volumes
            .IgnoreQueryFilters()
            .GroupBy(x => x.PurchaseUrl)
            .ToListAsync();

        foreach (var group in groupedByUrl)
        {
            if (group.Count() <= 1)
            {
                continue;
            }

            var toRemove = group.OrderBy(x => x.CreatedAt).Skip(1).ToList();
            context.Volumes.RemoveRange(toRemove);
            _logger.LogInformation($"Removed {toRemove.Count} duplicated volumes with PurchaseUrl '{group.Key}'");
        }
    }
}
