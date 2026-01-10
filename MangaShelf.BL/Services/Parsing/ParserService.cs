using MangaShelf.BL.Interfaces;
using MangaShelf.BL.Parsers;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using MangaShelf.DAL.System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MangaShelf.BL.Services.Parsing;

public class ParserService : IParseService
{
    private readonly ILogger<ParserService> _logger;
    private readonly IDbContextFactory<MangaDbContext> _dbContextFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly ParserServiceOptions _options;

    public ParserService(ILogger<ParserService> logger,
        IDbContextFactory<MangaDbContext> dbContextFactory,
        IServiceProvider serviceProvider,
        IParserFactory parserFactory,
        IOptions<ParserServiceOptions> options)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _serviceProvider = serviceProvider;
        _options = options.Value;
    }

    private async Task ParseSite(IPublisherParser parser, Guid jobId, CancellationToken token)
    {
        using var scope = _serviceProvider.CreateScope();
        var volumeService = scope.ServiceProvider.GetRequiredService<IVolumeService>();
        var statusService = scope.ServiceProvider.GetRequiredService<IParserWriteService>();
        var readService = scope.ServiceProvider.GetRequiredService<IParserReadService>();

        _logger.LogDebug($"{parser.GetType().Name}: Starting parsing");
        bool pageExists = true;
        var currentPage = 0;
        var volumesToParse = new List<string>();

        await statusService.RunJob(jobId, token);

        do
        {
            if (token.IsCancellationRequested)
            {
                _logger.LogInformation("Worker stopping due to cancellation request");
                return;
            }

            string pageUrl = string.Empty;
            IEnumerable<string> volumes = Enumerable.Empty<string>();

            try
            {
                pageUrl = parser.GetNextPageUrl();
                currentPage += 1;
                pageUrl = pageUrl.Replace("{0}", currentPage.ToString());

                _logger.LogDebug("accessing " + pageUrl);
                volumes = await parser.GetVolumesUrls(pageUrl, token);
            }
            catch (NotImplementedException ex)
            {
                pageExists = false;
                _logger.LogInformation($"{parser.GetType().Name}: {ex.Message}");
                await statusService.RecordErrorAndStop(jobId, ex, token);
                break;
            }
            catch (HttpRequestException httpEx) when (httpEx.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                pageExists = false;
                _logger.LogInformation($"{parser.GetType().Name}: Page not found {pageUrl} : {httpEx.Message}");
                await statusService.RecordError(jobId, pageUrl, httpEx, token);
                break;
            }
            catch (Exception ex)
            {
                pageExists = false;
                _logger.LogInformation($"{parser.GetType().Name}: Cannot get volumes from {pageUrl} : {ex.Message}");
                await statusService.RecordError(jobId, pageUrl, ex, token);
                break;
            }

            _logger.LogDebug($"found {volumes.Count()} volumes");

            if (volumes.Count() == 0)
            {
                pageExists = false;
                _logger.LogDebug($"{parser.GetType().Name}: No volumes found on page {pageUrl}, last page reached");
                break;
            }
            if(volumes.Except(volumesToParse).Count() == 0)
            {
                pageExists = false;
                _logger.LogDebug($"{parser.GetType().Name}: No new volumes found on page {pageUrl}, last page reached");
                break;
            }   

            volumesToParse.AddRange(volumes);

            var jobStatus = await readService.GetJobStatusById(jobId, token);
            if (jobStatus == RunStatus.Cancelled)
            {
                _logger.LogInformation("Job was cancelled, stopping parsing");
                return;
            }

            await Task.Delay(_options.DelayBetweenParse, token);

        } while (pageExists);


        if (_options.IgnoreExistingVolumes)
        {
            var filtered = await volumeService.FilterExistingVolumes(volumesToParse, token);
            volumesToParse = filtered.ToList();
            _logger.LogDebug($"{volumesToParse.Count()} volumes left after filtering");
        }

        await statusService.SetToParsingStatus(jobId, volumesToParse, token);

        var progress = 0.0;
        var step = 100.0 / volumesToParse.Count;

        foreach (var volume in volumesToParse)
        {
            if (token.IsCancellationRequested)
            {
                _logger.LogInformation("Worker stopping due to cancellation request");
                return;
            }
            var jobStatus = await readService.GetJobStatusById(jobId, token);
            if (jobStatus == RunStatus.Cancelled)
            {
                _logger.LogInformation("Job was cancelled, stopping parsing");
                return;
            }

            await Task.Delay(_options.DelayBetweenParse, token);

            var result = await ParsePageInternal(jobId, volume, parser, token);

            progress += step;
            await statusService.SetProgress(jobId, progress, volume, result == State.Updated, token);
        }

        await statusService.SetToFinishedStatus(jobId, token);

        _logger.LogDebug($"{parser.GetType().Name}: Finished parsing");
    }


    private async Task<State> ParsePageInternal(Guid jobId, string url, IPublisherParser parser, CancellationToken token)
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IParserWriteService>();

        await service.RunSingleJob(jobId);
        State result = default;

        ParsedInfo volumeInfo;
        try
        {
            _logger.LogDebug($"parsing {url}");
            volumeInfo = await parser.Parse(url, token);
        }
        catch (Exception ex)
        {
            await service.RecordError(jobId, url, ex, token: token);
            _logger.LogWarning($"{parser.GetType().Name}: Cannot parse volume {url} : {ex.Message}");
            return result;
        }

        try
        {
            result = await CreateOrUpdateFromParsedInfoAsync(volumeInfo, token);
        }
        catch (Exception ex)
        {
            await service.RecordError(jobId, url, volumeInfo.Json, ex, token);
            _logger.LogWarning($"{parser.GetType().Name}: Cannot create volume {volumeInfo.Series} - {volumeInfo.Title}: {ex.Message}");
            await service.SetSingleJobToErrorStatus(jobId);
            return result;
        }

        await service.SetSingleJobToFinishedStatus(jobId);
        return result;
    }

    public async Task<State> CreateOrUpdateFromParsedInfoAsync(ParsedInfo volumeInfo, CancellationToken token = default)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var domainContextFactory = new DomainServiceFactory(context);
        var countryDomainService = domainContextFactory.GetDomainService<ICountryDomainService>();
        var publisherDomainService = domainContextFactory.GetDomainService<IPublisherDomainService>();
        var seriesDomainService = domainContextFactory.GetDomainService<ISeriesDomainService>();
        var authorDomainService = domainContextFactory.GetDomainService<IAuthorDomainService>();
        var volumeDomainService = domainContextFactory.GetDomainService<IVolumeDomainService>();

        var country = await countryDomainService.GetByCountryCodeAsync(volumeInfo.CountryCode, token) ?? await countryDomainService.GetByCountryCodeAsync("uk", token);

        var publisher = await publisherDomainService.GetByNameAsync(volumeInfo.Publisher, token);
        if (publisher == null)
        {
            var uri = new Uri(volumeInfo.Url);
            string baseUrl = $"{uri.Scheme}://{uri.Host}";
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
                IsPublishedOnSite = series.IsPublishedOnSite
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

        volume.IsPreorder = volumeInfo.IsPreorder;

        if (volumeInfo.TotalVolumes > 0 && volumeInfo.TotalVolumes > volume.Series.TotalVolumes)
        {
            volume.Series.TotalVolumes = volumeInfo.TotalVolumes;
        }

        if (volumeInfo.Release is not null && volumeInfo.Release > DateTime.Now)
        {
            volume.ReleaseDate = volumeInfo.Release;
        }
        else if (!volume.IsPreorder && volumeInfo.PreorderStartDate != null)
        {
            volume.ReleaseDate = volumeInfo.PreorderStartDate.Value.AddDays(30);
        }
        else
        {
            volume.ReleaseDate = DateTimeOffset.Now;
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
            using var scope = _serviceProvider.CreateScope();
            var _imageManager = scope.ServiceProvider.GetRequiredService<IImageManager>();

            volume.OriginalCoverUrl = _imageManager.DownloadFileFromWeb(volumeInfo.Cover, volume.Series.PublicId);

            var croppedImage = _imageManager.CropImage(volume.OriginalCoverUrl);

            volume.CoverImageUrl = croppedImage ?? volume.OriginalCoverUrl;
            volume.CoverImageUrlSmall = _imageManager.CreateSmallImage(volume.CoverImageUrl);
        }

        var result = await volumeDomainService.AddOrUpdate(volume, true, token);
        _logger.LogInformation($"{result.Entity.Series.Title} {volume.Title} {volume.Number} was {result.State}");

        return result.State;
    }

    public async Task RunParseJob(Guid jobId, CancellationToken token)
    {
        using var scope = _serviceProvider.CreateScope();
        var statusService = scope.ServiceProvider.GetRequiredService<IParserReadService>();

        var job = await statusService.GetJobById(jobId);
        if (job == null)
        {
            throw new InvalidOperationException($"Job {jobId} not found");
        }

        if(job.Status == RunStatus.Cancelled)
        {
            return;
        }

        var factory = scope.ServiceProvider.GetRequiredService<IParserFactory>();
        var parsers = factory.GetParsers().SingleOrDefault(x => x.ParserName == job.ParserStatus.ParserName);

        if (parsers == null)
        {
            throw new InvalidOperationException($"Parser {job.ParserStatus.ParserName} not found");
        }

        try
        {
            switch (job.Type)
            {
                case ParserRunType.SingleUrl:

                    await ParsePageInternal(jobId, job.Url!, parsers, token);
                    break;
                case ParserRunType.FullSite:
                    await ParseSite(parsers, jobId, token);
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            var parserWriteService = scope.ServiceProvider.GetRequiredService<IParserWriteService>();
            await parserWriteService.RecordErrorAndStop(jobId, ex, token);
            throw;
        }
    }
}
