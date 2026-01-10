using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.DAL.DomainServices;

public class SeriesDomainService : BaseDomainService<Series>, ISeriesDomainService
{
    internal SeriesDomainService(MangaDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<string>> GetAllTitlesAsync(CancellationToken stoppingToken)
    {
        return await _context.Series.Select(x=>x.Title).ToListAsync(stoppingToken);
    }

    public async Task<Series?> GetByTitleAsync(string series, CancellationToken token = default)
    {
        return await _context.Series
            .FirstOrDefaultAsync(s => s.Title.ToLower() == series.ToLower(), token);
    }
}