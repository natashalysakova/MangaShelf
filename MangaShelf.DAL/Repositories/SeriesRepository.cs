using MangaShelf.DAL;
using MangaShelf.DAL.Models;
using MangaShelf.DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.Infrastructure.Installer;

public class SeriesRepository : BaseRepository<Series>, ISeriesRepository
{
    public SeriesRepository(MangaDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Series?> GetByTitle(string series)
    {
        return await _context.Series
            .FirstOrDefaultAsync(s => s.Title.ToLower() == series.ToLower());
    }
}