using MangaShelf.BL.Configuration;
using MangaShelf.BL.Contracts;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL;
using MangaShelf.DAL.System.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.Parser.Services;

public class ParserService : IParseService
{
    private readonly ILogger<ParserService> _logger;
    private readonly IDbContextFactory<MangaDbContext> _dbContextFactory;
    private readonly IParserFactory _parserFactory;
    private readonly IVolumeInfoParser _volumeInfoParser;
    private readonly IVolumeService _volumeService;
    private readonly IParseJobManagerService _jobManagerService;
    private readonly IParserReadService _parserReadService;
    private readonly ICacheInvalidator _cacheInvalidator;
    private readonly ParserServiceSettings _options;

    public ParserService(ILogger<ParserService> logger,
        IDbContextFactory<MangaDbContext> dbContextFactory,
        IParserFactory parserFactory,
        IConfigurationService configurationService,
        IVolumeInfoParser volumeInfoParser,
        IVolumeService volumeService,
        IParseJobManagerService jobManagerService,
        IParserReadService parserReadService,
        ICacheInvalidator cacheInvalidator)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _parserFactory = parserFactory;
        _volumeInfoParser = volumeInfoParser;
        _volumeService = volumeService;
        _jobManagerService = jobManagerService;
        _parserReadService = parserReadService;
        _cacheInvalidator = cacheInvalidator;
        _options = configurationService.ParserService;
    }

    private async Task ParseSite(IPublisherParser parser, Guid jobId, CancellationToken token)
    {
        _logger.LogDebug($"{parser.GetType().Name}: Starting parsing");

        await _jobManagerService.RunJob(jobId, token);

        var volumesToParse = await GetVolumesFromSite(parser, jobId, token);

        if (_options.IgnoreExistingVolumes)
        {
            var filtered = await _volumeService.FilterExistingVolumes(volumesToParse, token);
            volumesToParse = filtered.ToList();
            _logger.LogDebug($"{volumesToParse.Count()} volumes left after filtering");
        }

        if(!volumesToParse.Any())
        {
            _logger.LogDebug("No volumes to parse, finishing job");
            await _jobManagerService.SetToFinishedStatus(jobId, token);
            return;
        }

        await _jobManagerService.SetToParsingStatus(jobId, volumesToParse, token);

        var progress = 0.0;
        var step = 100.0 / volumesToParse.Count();

        foreach (var volume in volumesToParse)
        {
            if (token.IsCancellationRequested)
            {
                _logger.LogInformation("Worker stopping due to cancellation request");
                throw new OperationCanceledException();
            }

            var jobStatus = await _parserReadService.GetJobStatusById(jobId, token);
            if (jobStatus == RunStatus.Cancelled)
            {
                _logger.LogInformation("Job was cancelled, stopping parsing");
                throw new OperationCanceledException();
            }

            await Task.Delay(_options.DelayBetweenParse, token);

            ParseResult? result = default;
            try
            {
                result = await ParsePageInternal(jobId, volume, parser, token);
            }
            catch (JobFailedException ex)
            {
                await _jobManagerService.RecordError(jobId, ex.InnerException, ex.Url, token: token);
            }
            catch (Exception ex)
            {
                await _jobManagerService.RecordError(jobId, ex, volume, token: token);
            }
            finally
            {
                progress += step;
                await _jobManagerService.SetProgress(jobId, progress, result, token);
            }
        }

        await _jobManagerService.SetToFinishedStatus(jobId, token);

        _logger.LogDebug($"{parser.GetType().Name}: Finished parsing");
    }

    private async Task<IEnumerable<string>> GetVolumesFromSite(IPublisherParser parser, Guid jobId, CancellationToken token)
    {
        bool pageExists = true;
        var currentPage = 0;
        var volumesToParse = new List<string>();

        do
        {
            if (token.IsCancellationRequested)
            {
                _logger.LogInformation("Worker stopping due to cancellation request");
                throw new OperationCanceledException();
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
                throw new JobFailedException(pageUrl, ex);
            }
            catch (HttpRequestException httpEx) when (httpEx.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                pageExists = false;
                _logger.LogInformation($"{parser.GetType().Name}: Page not found {pageUrl} : {httpEx.Message}");
                await _jobManagerService.RecordError(jobId, httpEx, pageUrl, token);
                break;
            }
            catch (Exception ex)
            {
                pageExists = false;
                _logger.LogInformation($"{parser.GetType().Name}: Cannot get volumes from {pageUrl} : {ex.Message}");
                await _jobManagerService.RecordError(jobId, ex, pageUrl, token);
                break;
            }

            _logger.LogDebug($"found {volumes.Count()} volumes");

            if (volumes.Count() == 0)
            {
                pageExists = false;
                _logger.LogDebug($"{parser.GetType().Name}: No volumes found on page {pageUrl}, last page reached");
                break;
            }
            if (volumes.Except(volumesToParse).Count() == 0)
            {
                pageExists = false;
                _logger.LogDebug($"{parser.GetType().Name}: No new volumes found on page {pageUrl}, last page reached");
                break;
            }

            volumesToParse.AddRange(volumes);

            await Task.Delay(_options.DelayBetweenParse, token);

        } while (pageExists);

        return volumesToParse;
    }

    private async Task<ParseResult> ParsePageInternal(Guid jobId, string url, IPublisherParser parser, CancellationToken token)
    {
        ParsedInfo volumeInfo;
        try
        {
            _logger.LogDebug($"parsing {url}");
            volumeInfo = await parser.Parse(url, token);
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"{parser.GetType().Name}: Cannot parse volume {url} : {ex.Message}");
            throw new JobFailedException(url, ex);
        }

        try
        {
            return await _volumeInfoParser.Parse(volumeInfo, token);
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"{parser.GetType().Name}: Cannot create volume {volumeInfo.Series} - {volumeInfo.Title}: {ex.Message}");
            throw new JobFailedException(volumeInfo.Url, ex);
        }
    }

    private async Task<ParseResult> ParseSinglePage(Guid jobId, string url, IPublisherParser parser, CancellationToken token)
    {
        await _jobManagerService.RunJob(jobId, token);
        ParseResult result = await ParsePageInternal(jobId, url, parser, token);
        await _jobManagerService.SetToFinishedStatus(jobId, token);
        return result;
    }



    public async Task RunParseJob(Guid jobId, CancellationToken token)
    {
        var job = await _parserReadService.GetJobById(jobId, token);
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

                    await ParseSinglePage(jobId, job.Url!, parsers, token);
                    break;
                case ParserRunType.FullSite:
                    await ParseSite(parsers, jobId, token);
                    break;
                default:
                    break;
            }
        }
        catch (JobFailedException ex)
        {
            _logger.LogError(ex, $"Job {jobId} failed: {ex.Message}");
            await _jobManagerService.RecordErrorAndStop(jobId, ex.InnerException, ex.Url, token);
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Worker stopping due to cancellation request");
            await _jobManagerService.SetToCancelledStatus(jobId, token);
            throw;
        }
        catch (Exception ex)
        {
            await _jobManagerService.RecordErrorAndStop(jobId, ex, token: token);
            throw;
        }

        await _cacheInvalidator.RebuildCache(token);
    }
}
