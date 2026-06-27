using MangaShelf.BL.Mappers;
using MangaShelf.DAL;
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

        return histories.Select(vh => vh.ToDto()).ToList();
    }

    
}
