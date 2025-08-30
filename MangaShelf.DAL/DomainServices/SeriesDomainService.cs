using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.DAL.DomainServices;

public class SeriesDomainService : BaseDomainService<Series>, ISeriesDomainService
{
    public SeriesDomainService(MangaDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Series?> GetByTitle(string series)
    {
        return await _context.Series
            .FirstOrDefaultAsync(s => s.Title.ToLower() == series.ToLower());
    }
}