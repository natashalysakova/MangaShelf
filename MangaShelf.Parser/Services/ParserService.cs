using MangaShelf.BL.Configuration;
using MangaShelf.BL.Contracts;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.System.Models;

namespace MangaShelf.Parser.Services;

public class ParserService : IParseService
{
    private readonly ILogger<ParserService> _logger;
    private readonly IVolumeImportService _volumeImportService;
    private readonly IVolumeService _volumeService;
    private readonly IParserRunTracker _parserRunTracker;
    private readonly IParserReadService _parserReadService;
    private readonly IParserFactory _parserFactory;
    private readonly ICacheInvalidator _cacheInvalidator;
    private readonly ParserServiceSettings _options;

    public ParserService(
        ILogger<ParserService> logger,
        IVolumeImportService volumeImportService,
        IVolumeService volumeService,
        IParserRunTracker parserRunTracker,
        IParserReadService parserReadService,
        IParserFactory parserFactory,
        IConfigurationService configurationService,
        ICacheInvalidator cacheInvalidator)
    {
        _logger = logger;
        _volumeImportService = volumeImportService;
        _volumeService = volumeService;
        _parserRunTracker = parserRunTracker;
        _parserReadService = parserReadService;
        _parserFactory = parserFactory;
        _cacheInvalidator = cacheInvalidator;
        _options = configurationService.ParserService;
    }

    private async Task ParseSite(IPublisherParser parser, Guid jobId, CancellationToken token)
    {
        _logger.LogDebug($"{parser.GetType().Name}: Starting parsing");
        bool pageExists = true;
        var currentPage = 0;
        var volumesToParse = new List<string>();

        await _parserRunTracker.RunJob(jobId, token);

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
                currentPage += 1;
                pageUrl = parser.GetPageUrl(currentPage);

                _logger.LogDebug("accessing " + pageUrl);
                volumes = await parser.GetVolumesUrls(pageUrl, token);
            }
            catch (NotImplementedException ex)
            {
                pageExists = false;
                _logger.LogInformation($"{parser.GetType().Name}: {ex.Message}");
                await _parserRunTracker.RecordErrorAndStop(jobId, ex, token);
                break;
            }
            catch (HttpRequestException httpEx) when (httpEx.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                pageExists = false;
                _logger.LogInformation($"{parser.GetType().Name}: Page not found {pageUrl} : {httpEx.Message}");
                await _parserRunTracker.RecordError(jobId, pageUrl, httpEx, token);
                break;
            }
            catch (Exception ex)
            {
                pageExists = false;
                _logger.LogInformation($"{parser.GetType().Name}: Cannot get volumes from {pageUrl} : {ex.Message}");
                await _parserRunTracker.RecordError(jobId, pageUrl, ex, token);
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

            var jobStatus = await _parserReadService.GetJobStatusById(jobId, token);
            if (jobStatus == RunStatus.Cancelled)
            {
                _logger.LogInformation("Job was cancelled, stopping parsing");
                return;
            }

            await Task.Delay(_options.DelayBetweenParse, token);

        } while (pageExists);


        if (_options.IgnoreExistingVolumes)
        {
            var filtered = await _volumeService.FilterExistingVolumes(volumesToParse, token);
            volumesToParse = filtered.ToList();
            _logger.LogDebug($"{volumesToParse.Count()} volumes left after filtering");
        }

        if(volumesToParse.Count == 0)
        {
            _logger.LogDebug("No volumes to parse, finishing job");
            await _parserRunTracker.SetToFinishedStatus(jobId, token);
            return;
        }

        await _parserRunTracker.SetToParsingStatus(jobId, volumesToParse, token);

        var progress = 0.0;
        var step = 100.0 / volumesToParse.Count;

        foreach (var volume in volumesToParse)
        {
            if (token.IsCancellationRequested)
            {
                _logger.LogInformation("Worker stopping due to cancellation request");
                return;
            }
            var jobStatus = await _parserReadService.GetJobStatusById(jobId, token);
            if (jobStatus == RunStatus.Cancelled)
            {
                _logger.LogInformation("Job was cancelled, stopping parsing");
                return;
            }

            await Task.Delay(_options.DelayBetweenParse, token);

            var result = await ParsePageInternal(jobId, volume, parser, token);

            progress += step;
            await _parserRunTracker.SetProgress(jobId, progress, volume, result == State.Updated, token);
        }

        await _parserRunTracker.SetToFinishedStatus(jobId, token);

        _logger.LogDebug($"{parser.GetType().Name}: Finished parsing");
    }


    private async Task<State> ParsePageInternal(Guid jobId, string url, IPublisherParser parser, CancellationToken token)
    {
        await _parserRunTracker.RunSingleJob(jobId);
        State result = default;

        ParsedInfo volumeInfo;
        try
        {
            _logger.LogDebug($"parsing {url}");
            volumeInfo = await parser.Parse(url, token);
        }
        catch (Exception ex)
        {
            await _parserRunTracker.RecordError(jobId, url, ex, token);
            _logger.LogWarning($"{parser.GetType().Name}: Cannot parse volume {url} : {ex.Message}");
            return result;
        }

        try
        {
            result = await _volumeImportService.ImportAsync(volumeInfo, token);
        }
        catch (Exception ex)
        {
            await _parserRunTracker.RecordError(jobId, url, volumeInfo.Json, ex, token);
            _logger.LogWarning($"{parser.GetType().Name}: Cannot create volume {volumeInfo.Series} - {volumeInfo.Title}: {ex.Message}");
            await _parserRunTracker.SetSingleJobToErrorStatus(jobId);
            return result;
        }

        await _parserRunTracker.SetSingleJobToFinishedStatus(jobId);
        return result;
    }

    public async Task RunParseJob(Guid jobId, CancellationToken token)
    {
        var job = await _parserReadService.GetJobById(jobId);
        if (job == null)
        {
            throw new InvalidOperationException($"Job {jobId} not found");
        }

        if(job.Status == RunStatus.Cancelled)
        {
            return;
        }

        var parsers = _parserFactory.GetParsers().SingleOrDefault(x => x.ParserName == job.ParserStatus.ParserName);

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
            await _parserRunTracker.RecordErrorAndStop(jobId, ex, token);
            throw;
        }

        await _cacheInvalidator.RebuildCache(token);
    }
}

