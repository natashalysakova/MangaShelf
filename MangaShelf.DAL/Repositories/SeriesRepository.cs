using MangaShelf.DAL;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.DAL.Repositories
{
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
}