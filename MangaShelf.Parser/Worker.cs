using MangaShelf.BL.Interfaces;
using MangaShelf.BL.Parsers;
using MangaShelf.Common;
using MangaShelf.Common.Interfaces;

namespace MangaShelf.Parser;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {


        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            using var scope = _serviceProvider.CreateScope();

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var isEnabled = configuration.GetValue<bool>($"BackgroundWorker:Enabled");
            if (!isEnabled)
            {
                _logger.LogDebug("Background worker is disabled, exiting");
                return;
            }

            var delayBetweenRunsInHours = configuration.GetValue<int>("BackgroundWorker:DelayBetweenRuns");
            var delayBetweenRuns = delayBetweenRunsInHours * 60 * 60 * 1000;

            var taskList = new List<Task>();
            var parsers = new PublisherParsersFactory(scope.ServiceProvider).GetParsers();

            foreach (var parser in parsers)
            {
                var task = Task.Run(async () =>
                {
                    await Parse(parser, _serviceProvider, stoppingToken);
                }, stoppingToken);
                taskList.Add(task);
            }

            Task.WaitAll(taskList);

            await Task.Delay(delayBetweenRuns, stoppingToken);
        }
    }

    public async Task Parse(IPublisherParser parser, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var volumeService = scope.ServiceProvider.GetRequiredService<IVolumeService>();
        var parsedVolumeService = scope.ServiceProvider.GetRequiredService<IParsedVolumeService>();
        var service = scope.ServiceProvider.GetRequiredService<IFailedSyncRecordsService>();

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var delayBetweenParseInSeconds = configuration.GetValue<int>("BackgroundWorker:DelayBetweenParse");
        var delayBetweenParse = delayBetweenParseInSeconds * 1000;

        _logger.LogDebug($"{parser.GetType().Name}: Starting parsing");
        bool pageExists = true;
        var currentPage = 0;
        do
        {
            var pageUrl = parser.GetNextPageUrl();

            currentPage += 1;
            pageUrl = pageUrl.Replace("{0}", currentPage.ToString());

            var volumesToParse = Enumerable.Empty<string>();
            try
            {
                _logger.LogDebug("accessing " + pageUrl);
                volumesToParse = await parser.GetVolumesUrls(pageUrl);
            }
            catch (Exception ex)
            {
                pageExists = false;
                _logger.LogInformation($"{parser.GetType().Name}: Cannot get volumes from {pageUrl} : {ex.Message}");
                break;
            }

            _logger.LogDebug($"found {volumesToParse.Count()} volumes");
            if (volumesToParse.Count() == 0)
            {
                pageExists = false;
                _logger.LogDebug($"{parser.GetType().Name}: No volumes found on page {pageUrl}, stopping");
                break;
            }

            var ignoreExisting = configuration.GetValue<bool>("BackgroundWorker:IgnoreExistingVolumes");
            if (ignoreExisting)
            {
                volumesToParse = await volumeService.FilterExistingVolumes(volumesToParse);
                _logger.LogDebug($"{volumesToParse.Count()} volumes left after filtering");
            }

            foreach (var volume in volumesToParse)
            {
                await Task.Delay(delayBetweenParse, cancellationToken);

                ParsedInfo volumeInfo;
                try
                {
                    _logger.LogDebug($"parsing {volume}");
                    volumeInfo = await parser.Parse(volume);
                }
                catch (Exception ex)
                {
                    service.CreateFailedSyncRecord(pageUrl, parser.GetType().Name, exception: ex);
                    _logger.LogWarning($"{parser.GetType().Name}: Cannot parse volume {volume} : {ex.Message}");
                    continue;
                }

                try
                {
                    await parsedVolumeService.CreateOrUpdateFromParsedInfoAsync(volumeInfo);
                }
                catch (Exception ex)
                {
                    service.CreateFailedSyncRecord(pageUrl, parser.GetType().Name, volumeJson: volumeInfo.json,  exception: ex);
                    _logger.LogWarning($"{parser.GetType().Name}: Cannot create volume {volumeInfo.series} - {volumeInfo.title}: {ex.Message}");
                    continue;
                }
            }

        } while (pageExists);

        _logger.LogDebug($"{parser.GetType().Name}: Finished parsing");
    } 
}