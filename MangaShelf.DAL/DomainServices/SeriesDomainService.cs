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
        var titles = await _context.Series
            .Where(s => !string.IsNullOrEmpty(s.Title))
            .Select(s => s.Title)
            .ToListAsync(stoppingToken);

        var originalTitles = await _context.Series
            .Where(s => !string.IsNullOrEmpty(s.OriginalTitle))
            .Select(s => s.OriginalTitle)   
            .Distinct().ToListAsync(stoppingToken);

        return titles.Concat(originalTitles).Distinct()!;
    }

    public async Task<Series?> GetByTitleAsync(string series, CancellationToken token = default)
    {
        return await _context.Series
            .FirstOrDefaultAsync(s => s.Title.ToLower() == series.ToLower(), token);
    }
}