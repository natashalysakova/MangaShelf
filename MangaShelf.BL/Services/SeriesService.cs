using MangaShelf.BL.Interfaces;
using MangaShelf.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using MangaShelf.BL.Mappers;
using MangaShelf.BL.Dto;

namespace MangaShelf.BL.Services;

public class SeriesService : ISeriesService
{
    private readonly ILogger<SeriesService> _logger;
    private readonly ISeriesDomainService _seriesRepository;
    private readonly IAuthorService _authorsService;
    private readonly IPublisherService _publisherService;

    public SeriesService(ILogger<SeriesService> logger, ISeriesDomainService seriesRepository, IAuthorService authorsService, IPublisherService publisherService)
    {
        _logger = logger;
        _seriesRepository = seriesRepository;
        _authorsService = authorsService;
        _publisherService = publisherService;
    }


    public async Task<SeriesDto?> FindByName(string seriesTitle)
    {
        var series = await _seriesRepository.GetByTitle(seriesTitle);
        return series?.ToDto();
    }
}
