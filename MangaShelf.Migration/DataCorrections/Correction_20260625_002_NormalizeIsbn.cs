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
