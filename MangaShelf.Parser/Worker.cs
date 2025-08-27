using MangaShelf.BL.Interfaces;
using MangaShelf.BL.Parsers;

namespace MangaShelf.Parser;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly int delayBetweenRuns = 24 * 60 * 60 * 1000; // 24 hour 
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
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var volumeService = scope.ServiceProvider.GetRequiredService<IVolumeService>();

            var parsers = new PublisherParsersFactory(loggerFactory).GetParsers();
            foreach (var parser in parsers)
            {
                bool pageExists = true;
                do
                {
                    var pageUrl = parser.GetNextPageUrl();
                    var volumesToParse = Enumerable.Empty<string>();
                    try
                    {
                        volumesToParse = await parser.GetVolumesUrls(pageUrl);
                    }
                    catch (Exception ex)
                    {
                        pageExists = false;
                        _logger.LogInformation($"{parser.GetType().Name}: Cannot get volumes from {pageUrl} : {ex.Message}");
                        break;
                    }

                    volumesToParse = await volumeService.FilterExistingVolumes(volumesToParse);

                    foreach (var volume in volumesToParse)
                    {
                        await Task.Delay(5000);

                        ParsedInfo volumeInfo;
                        try
                        {
                            volumeInfo = await parser.Parse(volume);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"{parser.GetType().Name}: Cannot parse volume {volume} : {ex.Message}");
                            continue;
                        }

                        try
                        {
                            await volumeService.CreateOrUpdateFromParsedInfoAsync(volumeInfo);
                        }
                        catch (Exception ex)
                        {
                            
                            _logger.LogWarning($"{parser.GetType().Name}: Cannot create volume {volumeInfo.series} - {volumeInfo.title}: {ex.Message}");
                            continue;
                        }
                    }

                } while (pageExists);
            }

            await Task.Delay(delayBetweenRuns, stoppingToken);
        }
    }
}
