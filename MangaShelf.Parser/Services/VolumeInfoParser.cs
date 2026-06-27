using MangaShelf.BL.Contracts;
using MangaShelf.Common.Helpers;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.Parser.Services;

public interface IVolumeInfoParser
{
    Task<State> Parse(ParsedInfo volumeInfo, CancellationToken token = default);
}
public class VolumeInfoParser(
    IDbContextFactory<MangaDbContext> dbContextFactory,
    ILogger<VolumeInfoParser> logger,
    IImageFlow imageFlow) : IVolumeInfoParser
{
    public async Task<State> Parse(ParsedInfo volumeInfo, CancellationToken token = default)
    {
        using var context = await dbContextFactory.CreateDbContextAsync();
        var domainContextFactory = new DomainServiceFactory(context);

        var country = await GetCountry(domainContextFactory, volumeInfo.CountryCode, token);
        var publisher = await GetOrCreatePublisher(domainContextFactory, volumeInfo, country, token);
        var series = await GetOrCreateSeries(domainContextFactory, volumeInfo, publisher, token);
        var volume = await GetOrCreateVolume(domainContextFactory, volumeInfo, series, token);

        UpdateVolumeProperties(volumeInfo, volume);
        await HandleImages(volumeInfo, volume);

        var result = await domainContextFactory.GetDomainService<IVolumeDomainService>().AddOrUpdate(volume, true, token);
        logger.LogInformation($"{volume.GetFullVolumeName()} {volume.Number} was {result.State}");

        return result.State;
    }

    // Helper methods
    private async Task<Country> GetCountry(DomainServiceFactory factory, string countryCode, CancellationToken token)
    {
        var countryService = factory.GetDomainService<ICountryDomainService>();
        return await countryService.GetByCountryCodeAsync(countryCode, token)
            ?? await countryService.GetByCountryCodeAsync("uk", token);
    }

    private async Task<Publisher> GetOrCreatePublisher(DomainServiceFactory factory, ParsedInfo volumeInfo, Country country, CancellationToken token)
    {
        var publisherService = factory.GetDomainService<IPublisherDomainService>();
        return await publisherService.GetByNameAsync(volumeInfo.Publisher, token)
            ?? new Publisher
            {
                Name = volumeInfo.Publisher,
                Country = country,
                Url = new Uri(volumeInfo.Url).ToString()
            };
    }

    private async Task<Series> GetOrCreateSeries(DomainServiceFactory factory, ParsedInfo volumeInfo, Publisher publisher, CancellationToken token)
    {
        var seriesService = factory.GetDomainService<ISeriesDomainService>();
        var series = await seriesService.GetByTitleAsync(volumeInfo.Series, volumeInfo.SeriesType, token);

        if (series != null) return series;

        var authors = await GetSeriesAuthors(factory, volumeInfo, token);

        return new Series
        {
            OriginalTitle = volumeInfo.OriginalSeriesTitle,
            Status = volumeInfo.SeriesStatus,
            TotalVolumes = volumeInfo.TotalVolumes,
            Title = volumeInfo.Series,
            Publisher = publisher,
            IsPublishedOnSite = volumeInfo.CanBePublished,
            Type = volumeInfo.SeriesType,
            Authors = authors
        };
    }

    private async Task<List<Author>> GetSeriesAuthors(DomainServiceFactory factory, ParsedInfo volumeInfo, CancellationToken token)
    {
        if (volumeInfo.Authors is null) return [];

        var authorsList = volumeInfo.Authors
            .Split([',', ';', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(a => a.Trim())
            .ToList();
        var authors = await factory.GetDomainService<IAuthorDomainService>().GetOrCreateByNames(authorsList, token);
        return authors.ToList();
    }

    private async Task<Volume> GetOrCreateVolume(DomainServiceFactory factory, ParsedInfo volumeInfo, Series series, CancellationToken token)
    {
        var volumeService = factory.GetDomainService<IVolumeDomainService>();
        var volume = volumeService.FindVolumeFromParsedInfo(series.Id, volumeInfo.ToVolumeInfoRequest());

        return volume ?? new Volume
        {
            Title = volumeInfo.Title,
            Series = series,
            ISBN = volumeInfo.Isbn,
            Number = volumeInfo.VolumeNumber,
            AgeRestriction = volumeInfo.AgeRestrictions ?? 18,
            IsPublishedOnSite = series.IsPublishedOnSite,
            ReleaseDate = MapReleaseDate(volumeInfo.Release),
            Type = volumeInfo.VolumeType,
            PurchaseUrl = volumeInfo.Url,
        };
    }

    private void UpdateVolumeProperties(ParsedInfo volumeInfo, Volume volume)
    {
        UpdateDescription(volumeInfo, volume);
        UpdateUrl(volumeInfo, volume);
        UpdateSeriesStatus(volumeInfo, volume);
        UpdateTotalVolumes(volumeInfo, volume);
        UpdatePreorderInfo(volumeInfo, volume);
        UpdateAgeRestriction(volumeInfo, volume);
        UpdateIsbn(volumeInfo, volume);
        UpdateVolumeType(volumeInfo, volume);
    }

    private void UpdateDescription(ParsedInfo volumeInfo, Volume volume)
    {
        var currentValue = volume.Description;
        if (volumeInfo.Description != null && !Equals(currentValue, volumeInfo.Description))
            volume.Description = volumeInfo.Description;
    }

    private void UpdateUrl(ParsedInfo volumeInfo, Volume volume)
    {
        if (volume.PurchaseUrl != volumeInfo.Url && volumeInfo.Url.Contains(volume.Series.Publisher.Url))
            volume.PurchaseUrl = volumeInfo.Url;
    }

    private void UpdateSeriesStatus(ParsedInfo volumeInfo, Volume volume)
    {
        if (volumeInfo.SeriesStatus != SeriesStatus.Unknown && volumeInfo.SeriesStatus != volume.Series.Status)
            volume.Series.Status = volumeInfo.SeriesStatus;
    }

    private void UpdateTotalVolumes(ParsedInfo volumeInfo, Volume volume)
    {
        if (volumeInfo.TotalVolumes != null && (volume.Series.TotalVolumes == null || volumeInfo.TotalVolumes > volume.Series.TotalVolumes))
            volume.Series.TotalVolumes = volumeInfo.TotalVolumes;
    }

    private void UpdatePreorderInfo(ParsedInfo volumeInfo, Volume volume)
    {
        var wasPreorder = volume.IsPreorder;
        volume.IsPreorder = volumeInfo.IsPreorder;

        if (volumeInfo.IsPreorder && volumeInfo.Release != null)
            volume.ReleaseDate = MapReleaseDate(volumeInfo.Release);
        else if (!volumeInfo.IsPreorder && wasPreorder)
            volume.ReleaseDate = DateTimeOffset.UtcNow;

        if (volume.IsPreorder && volume.PreorderStart == null)
            volume.PreorderStart = DateTimeOffset.UtcNow;
    }

    private void UpdateAgeRestriction(ParsedInfo volumeInfo, Volume volume)
    {
        if (volumeInfo.AgeRestrictions != null && volume.AgeRestriction != volumeInfo.AgeRestrictions)
            volume.AgeRestriction = volumeInfo.AgeRestrictions.Value;
    }

    private void UpdateIsbn(ParsedInfo volumeInfo, Volume volume)
    {
        if (volume.ISBN is null && volumeInfo.Isbn is not null)
            volume.ISBN = volumeInfo.Isbn;
    }

    private void UpdateVolumeType(ParsedInfo volumeInfo, Volume volume)
    {
        if (volume.Type == VolumeType.NotSpecified && volumeInfo.VolumeType != VolumeType.NotSpecified)
            volume.Type = volumeInfo.VolumeType;
    }

    private async Task HandleImages(ParsedInfo volumeInfo, Volume volume)
    {
        if (string.IsNullOrEmpty(volumeInfo.Cover) || volume.OriginalCoverUrl is not null)
            return;

        try
        {
            var imageResult = await imageFlow.DownloadAndProcessImage(volumeInfo.Cover, volume.Series!.PublicId);
            volume.OriginalCoverUrl = imageResult?.OriginalImage;
            volume.CoverImageUrl = imageResult?.CroppedImage ?? volume.OriginalCoverUrl;
            volume.CoverImageUrlSmall = imageResult?.SmallImage;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to process image for volume {volume.GetFullVolumeName()}");
            return;
        }
    }

    private DateTimeOffset MapReleaseDate(DateTimeOffset? release)
    {
        if (release.HasValue)
        {
            return new DateTimeOffset(release.Value.DateTime, DateTimeOffset.Now.Offset);
        }

        return DateTimeOffset.Now;
    }
}