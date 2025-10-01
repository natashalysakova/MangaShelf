using MangaShelf.BL.Interfaces;
using MangaShelf.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using MangaShelf.BL.Mappers;
using MangaShelf.BL.Dto;
using Microsoft.EntityFrameworkCore;
using MangaShelf.DAL;

namespace MangaShelf.BL.Services;

public class SeriesService : ISeriesService
{
    private readonly ILogger<SeriesService> _logger;
    private readonly IDbContextFactory<MangaDbContext> _contextFactory;

    public SeriesService(ILogger<SeriesService> logger, IDbContextFactory<MangaDbContext> contextFactory)
    {
        _logger = logger;
        _contextFactory = contextFactory;
    }


    public async Task<SeriesSimpleDto?> FindByName(string seriesTitle)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var serviceFactory = new DomainServiceFactory(context);
        var seriesService = serviceFactory.GetDomainService<ISeriesDomainService>();

        var series = await seriesService.GetByTitleAsync(seriesTitle);
        return series?.ToDto();
    }

    public async Task<IEnumerable<string>> GetAllTitlesAsync(CancellationToken stoppingToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var serviceFactory = new DomainServiceFactory(context);
        var seriesService = serviceFactory.GetDomainService<ISeriesDomainService>();

        var titles = await seriesService.GetAllTitlesAsync(stoppingToken);
        return titles;
    }
}
