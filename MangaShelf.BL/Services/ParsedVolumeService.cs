using MangaShelf.BL.Interfaces;
using MangaShelf.Common;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Services;

public class ParsedVolumeService : IParsedVolumeService
{
    private readonly ILogger<ParsedVolumeService> _logger;
    private readonly ICountryDomainService _countryDomainService;
    private readonly IVolumeDomainService _volumeDomainService;
    private readonly ISeriesDomainService _seriesDomainService;
    private readonly IPublisherDomainService _publisherDomainService;
    private readonly IAuthorDomainService _authorDomainService;
    private readonly IImageManager _imageManager;

    public ParsedVolumeService(
        ILogger<ParsedVolumeService> logger,
        ICountryDomainService countryDomainService,
        IVolumeDomainService volumeDomainService,
        ISeriesDomainService seriesDomainService,
        IPublisherDomainService publisherDomainService,
        IAuthorDomainService authorDomainService,
        IImageManager imageManager)
    {
        _logger = logger;
        _countryDomainService = countryDomainService;
        _volumeDomainService = volumeDomainService;
        _seriesDomainService = seriesDomainService;
        _publisherDomainService = publisherDomainService;
        _authorDomainService = authorDomainService;
        _imageManager = imageManager;
    }

    public async Task CreateOrUpdateFromParsedInfoAsync(ParsedInfo volumeInfo)
    {
        var country = await _countryDomainService.GetByCountryCodeAsync(volumeInfo.countryCode) ?? await _countryDomainService.GetByCountryCodeAsync("uk");

        var publisher = await _publisherDomainService.GetByName(volumeInfo.publisher);
        if (publisher == null)
        {
            var uri = new Uri(volumeInfo.url);
            string baseUrl = $"{uri.Scheme}://{uri.Host}";
            publisher = new Publisher()
            {
                Name = volumeInfo.publisher,
                Country = country,
                Url = new Uri(volumeInfo.url).ToString()
            };
        }

        var series = await _seriesDomainService.GetByTitle(volumeInfo.series);
        if (series == null)
        {
            series = new Series()
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
                series.Authors = await _authorDomainService.GetOrCreateByNames(autorsList);
            }
        }

        var volume = await _volumeDomainService.FindBySeriesNameTitleAndNumber(volumeInfo.series, volumeInfo.volumeNumber, volumeInfo.title);
        if (volume == null)
        {
            volume = new Volume()
            {
                Title = volumeInfo.title,
                Series = series,
                ISBN = volumeInfo.isbn,
                Number = volumeInfo.volumeNumber,
                PreorderStart = volumeInfo.preorderStartDate,
                OneShot = volumeInfo.type.Equals("oneshot"),
                AgeRestriction = volumeInfo.ageRestrictions == null ? 18 : volumeInfo.ageRestrictions.Value,
            };
        }

        if (volume.PurchaseUrl is null || volume.PurchaseUrl != volumeInfo.url)
        {
            volume.PurchaseUrl = volumeInfo.url;
        }

        volume.Series.Ongoing = volumeInfo.seriesStatus == "ongoing";
        volume.IsPreorder = volumeInfo.isPreorder;

        if (volumeInfo.totalVolumes > 0 && volumeInfo.totalVolumes > volume.Series.TotalVolumes)
        {
            volume.Series.TotalVolumes = volumeInfo.totalVolumes;
        }

        if (volumeInfo.release is not null && volumeInfo.release < DateTime.Now)
        {
            volume.ReleaseDate = volumeInfo.release;
        }
        else if (!volume.IsPreorder && volumeInfo.preorderStartDate != null)
        {
            volume.ReleaseDate = volumeInfo.preorderStartDate.Value.AddDays(30);
        }
        else
        {
            volume.ReleaseDate = DateTime.Now;
        }

        if (volume.IsPreorder && volume.PreorderStart == null)
        {
            volume.PreorderStart = DateTimeOffset.Now;
        }

        if (volumeInfo.ageRestrictions != null && volume.AgeRestriction != volumeInfo.ageRestrictions)
        {
            volume.AgeRestriction = volumeInfo.ageRestrictions.Value;
        }

        if (volume.CoverImageUrl is null)
        {
            volume.CoverImageUrl = _imageManager.DownloadFileFromWeb(volumeInfo.cover);
        }

        var result = await _volumeDomainService.AddOrUpdate(volume);
        _logger.LogInformation($"{result.Item1.Series.Title} {volume.Title} {volume.Number} was {result.Item2}");

    }


    public async Task CreateVolume(ParsedInfo volumeInfo)
    {


    }

}
