using MangaShelf.BL.Interfaces;
using MangaShelf.BL.Services;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.Logging;

namespace MangaShelf.Infrastructure.Installer;

public class SeriesService : ISeriesService
{
    private readonly ILogger<SeriesService> _logger;
    private readonly ISeriesRepository _seriesRepository;
    private readonly IAuthorService _authorsService;
    private readonly IPublisherService _publisherService;

    public SeriesService(ILogger<SeriesService> logger, ISeriesRepository seriesRepository, IAuthorService authorsService, IPublisherService publisherService)
    {
        _logger = logger;
        _seriesRepository = seriesRepository;
        _authorsService = authorsService;
        _publisherService = publisherService;
    }

    public async Task<Series> CreateFromParsedVolumeInfo(ParsedInfo volumeInfo)
    {
        var publisher = await _publisherService.GetByName(volumeInfo.publisher);
        if (publisher is null)
        {
            publisher = await _publisherService.CreateFromParsedVolumeInfo(volumeInfo);
        }


        var series = new Series()
        {
            OriginalName = volumeInfo.originalSeriesName,
            Ongoing = volumeInfo.seriesStatus == "ongoing",
            TotalVolumes = volumeInfo.totalVolumes,
            Title = volumeInfo.series,
            Publisher = publisher
        };

        if (volumeInfo.authors is not null)
        {
            var autorsList = volumeInfo.authors.Split([',', ';', '\n'], StringSplitOptions.RemoveEmptyEntries);
            series.Authors = await _authorsService.GetByNames(autorsList, true);
        }

        await _seriesRepository.Add(series);
        return series;
    }

    public Task<Series?> FindByName(string series)
    {
        return _seriesRepository.GetByTitle(series);
    }
}
