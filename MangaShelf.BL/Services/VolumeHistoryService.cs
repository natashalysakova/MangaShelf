using MangaShelf.DAL;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.BL.Services;
public class VolumeHistoryService : IVolumeHistoryService
{
    private readonly IDbContextFactory<MangaDbContext> _dbContextFactory;

    public VolumeHistoryService(IDbContextFactory<MangaDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IEnumerable<VolumeHistoryDto>> GetVolumeHistoryAsync(DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var histories = await context.VolumeHistory
            .Include(vh => vh.Volume)
                .ThenInclude(v => v!.Series)
            .Where(vh => vh.Timestamp.Date >= from && vh.Timestamp.Date <= to)
            .ToListAsync(cancellationToken);

        return histories.Select(vh => new VolumeHistoryDto
        {
            VolumePublicId = vh.Volume!.PublicId,
            FullVolumeName = GetFullVolumeName(vh.Volume!),
            Date = vh.Timestamp.DateTime,
            EventType = vh.EventType,
            OldValue = vh.OldValue,
            NewValue = vh.NewValue
        }).ToList();
    }

    private static string GetFullVolumeName(Volume volume)
    {
        if (volume.OneShot)
        {
            return volume.Series!.Title;
        }

        return $"{volume.Series!.Title} - {volume.Title}";
    }
}
