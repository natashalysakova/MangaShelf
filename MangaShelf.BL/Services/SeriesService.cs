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

    public async Task<IEnumerable<SeriesWithVolumesDto>> GetAllWithVolumesAsync(CancellationToken token = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var series = await context.Series
            .Include(s => s.Authors)
            .Include(s => s.Publisher)
            .Include(s => s.Volumes)
            .OrderBy(s => s.Title)
            .ToListAsync(token);

        return series.Select(s => new SeriesWithVolumesDto
        {
            Id = s.Id,
            PublicId = s.PublicId,
            Title = s.Title,
            OriginalName = s.OriginalName,
            Aliases = s.Aliases,
            MalId = s.MalId,
            Type = s.Type,
            Status = s.Status,
            TotalVolumes = s.TotalVolumes,
            IsPublishedOnSite = s.IsPublishedOnSite,
            PublisherId = s.PublisherId,
            Publisher = s.Publisher?.ToDto() ?? new PublisherSimpleDto { Name = string.Empty },
            Authors = s.Authors.Select(a => a.Name).ToList(),
            Volumes = s.Volumes.OrderBy(v => v.Number).Select(v => v.ToDto()).ToList()
        });
    }

    public async Task UpdateSeriesAsync(SeriesUpdateDto dto, CancellationToken token = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var series = await context.Series
            .Include(s => s.Authors)
            .FirstOrDefaultAsync(s => s.Id == dto.Id, token);

        if (series is null)
        {
            _logger.LogWarning("Series {Id} not found for update", dto.Id);
            return;
        }

        series.Title = dto.Title;
        series.OriginalName = dto.OriginalName;
        series.Status = dto.Status;
        series.TotalVolumes = dto.TotalVolumes;

        var serviceFactory = new DomainServiceFactory(context);
        var authorService = serviceFactory.GetDomainService<IAuthorDomainService>();
        var resolvedAuthors = await authorService.GetOrCreateByNames(dto.Authors, token);

        series.Authors.Clear();
        foreach (var author in resolvedAuthors)
            series.Authors.Add(author);

        await context.SaveChangesAsync(token);
        _logger.LogInformation("Updated series {Id} ({Title})", series.Id, series.Title);
    }
}
