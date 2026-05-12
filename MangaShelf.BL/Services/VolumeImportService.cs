using MangaShelf.BL.Contracts;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Services;

public class VolumeImportService : IVolumeImportService
{
    private readonly ILogger<VolumeImportService> _logger;
    private readonly IDbContextFactory<MangaDbContext> _dbContextFactory;
    private readonly IImageManager _imageManager;

    public VolumeImportService(
        ILogger<VolumeImportService> logger,
        IDbContextFactory<MangaDbContext> dbContextFactory,
        IImageManager imageManager)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _imageManager = imageManager;
    }

    public async Task<State> ImportAsync(ParsedInfo volumeInfo, CancellationToken token = default)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync(token);
        var domainContextFactory = new DomainServiceFactory(context);
        var countryDomainService = domainContextFactory.GetDomainService<ICountryDomainService>();
        var publisherDomainService = domainContextFactory.GetDomainService<IPublisherDomainService>();
        var seriesDomainService = domainContextFactory.GetDomainService<ISeriesDomainService>();
        var authorDomainService = domainContextFactory.GetDomainService<IAuthorDomainService>();
        var volumeDomainService = domainContextFactory.GetDomainService<IVolumeDomainService>();

        var country = await countryDomainService.GetByCountryCodeAsync(volumeInfo.CountryCode, token)
                      ?? await countryDomainService.GetByCountryCodeAsync("uk", token);

        var publisher = await publisherDomainService.GetByNameAsync(volumeInfo.Publisher, token);
        if (publisher == null)
        {
            publisher = new Publisher()
            {
                Name = volumeInfo.Publisher,
                Country = country,
                Url = new Uri(volumeInfo.Url).ToString()
            };
        }

        var series = await seriesDomainService.GetByTitleAsync(volumeInfo.Series, token);
        if (series == null)
        {
            series = new Series()
            {
                OriginalName = volumeInfo.OriginalSeriesName,
                Status = volumeInfo.SeriesStatus,
                TotalVolumes = volumeInfo.TotalVolumes,
                Title = volumeInfo.Series,
                Publisher = publisher,
                IsPublishedOnSite = volumeInfo.CanBePublished,
                Type = volumeInfo.SeriesType
            };

            if (volumeInfo.Authors is not null)
            {
                var autorsList = volumeInfo.Authors.Split([',', ';', '\n'], StringSplitOptions.RemoveEmptyEntries);
                var authors = await authorDomainService.GetOrCreateByNames(autorsList, token);
                series.Authors = authors.ToList();
            }
        }

        var volume = volumeDomainService.FindBySeriesNameTitleAndNumber(volumeInfo.Series, volumeInfo.VolumeNumber, volumeInfo.Title);
        if (volume == null)
        {
            volume = new Volume()
            {
                Title = volumeInfo.Title,
                Series = series,
                ISBN = volumeInfo.Isbn,
                Number = volumeInfo.VolumeNumber,
                PreorderStart = volumeInfo.PreorderStartDate,
                OneShot = volumeInfo.SeriesStatus == SeriesStatus.OneShot,
                AgeRestriction = volumeInfo.AgeRestrictions == null ? 18 : volumeInfo.AgeRestrictions.Value,
                IsPublishedOnSite = series.IsPublishedOnSite,
                ReleaseDate = volumeInfo.Release,
            };
        }

        if (volumeInfo.Description is not null && volume.Description != volumeInfo.Description)
        {
            volume.Description = volumeInfo.Description;
        }

        if (volume.PurchaseUrl is null || volume.PurchaseUrl != volumeInfo.Url)
        {
            volume.PurchaseUrl = volumeInfo.Url;
        }

        if (volumeInfo.SeriesStatus != volume.Series.Status)
        {
            volume.Series.Status = volumeInfo.SeriesStatus;
        }

        if (volumeInfo.TotalVolumes > 0 && volumeInfo.TotalVolumes > volume.Series.TotalVolumes)
        {
            volume.Series.TotalVolumes = volumeInfo.TotalVolumes;
        }

        var wasPreorder = volume.IsPreorder;
        volume.IsPreorder = volumeInfo.IsPreorder;

        if (volumeInfo.IsPreorder && volumeInfo.Release != null)
        {
            volume.ReleaseDate = volumeInfo.Release;
        }
        else if (!volumeInfo.IsPreorder && wasPreorder)
        {
            volume.ReleaseDate = DateTimeOffset.UtcNow;
        }

        if (volume.IsPreorder && volume.PreorderStart == null)
        {
            volume.PreorderStart = DateTimeOffset.Now;
        }

        if (volumeInfo.AgeRestrictions != null && volume.AgeRestriction != volumeInfo.AgeRestrictions)
        {
            volume.AgeRestriction = volumeInfo.AgeRestrictions.Value;
        }

        if (!string.IsNullOrEmpty(volumeInfo.Cover) && volume.OriginalCoverUrl is null)
        {
            volume.OriginalCoverUrl = _imageManager.DownloadFileFromWeb(volumeInfo.Cover, volume.Series.PublicId);

            var croppedImage = _imageManager.CropImage(volume.OriginalCoverUrl);

            volume.CoverImageUrl = croppedImage ?? volume.OriginalCoverUrl;
            volume.CoverImageUrlSmall = _imageManager.CreateSmallImage(volume.CoverImageUrl);
        }

        var result = await volumeDomainService.AddOrUpdate(volume, true, token);
        _logger.LogInformation($"{result.Entity.Series.Title} {volume.Title} {volume.Number} was {result.State}");

        return result.State;
    }
}
