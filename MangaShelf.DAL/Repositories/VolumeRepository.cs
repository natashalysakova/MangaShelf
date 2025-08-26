using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MangaShelf.DAL.Repositories;

public class VolumeRepository : BaseRepository<Volume>, IVolumeRepository
{
    public VolumeRepository(ILogger<VolumeRepository> logger, MangaDbContext context) : base(context)
    {
        
    }

    public async Task<Volume?> FindBySeriesNameAndNumber(string series, int volumeNumber)
    {
        return await _context.Volumes
            .Include(x=>x.Series)
            .FirstOrDefaultAsync(x => x.Series!.Title == series && x.Number == volumeNumber);
    }
}
