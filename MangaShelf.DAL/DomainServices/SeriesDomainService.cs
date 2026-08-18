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

    public async Task<Series?> GetByTitleAsync(string series, SeriesType seriesType, CancellationToken token = default)
    {
        var query = _context.Series
            .Where(s => s.Title.ToLower() == series.ToLower());

        if(seriesType != SeriesType.Unknown)
        {
            query = query.Where(x => x.Type == seriesType);
        }

        return await query.FirstOrDefaultAsync(token);
    }
}