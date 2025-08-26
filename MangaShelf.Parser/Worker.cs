using MangaShelf.BL.Interfaces;
using MangaShelf.Parser.VolumeParsers;

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

            var parsers = new PublisherParsersFactory().GetParsers();

            foreach (var parser in parsers)
            {
                bool pageExists = true;
                do
                {
                    var pageUrl = parser.GetNextPageUrl();
                    try
                    {
                        var volumes = await parser.GetVolumesUrls(pageUrl);
                        foreach (var volume in volumes)
                        {
                            var volumeInfo = await parser.Parse(volume);
                            using var scope = _serviceProvider.CreateScope();
                            var volumeService = scope.ServiceProvider.GetRequiredService<IVolumeService>();
                            await volumeService.CreateOrUpdateFromParsedInfoAsync(volumeInfo);
                            await Task.Delay(5000);
                        }
                    }
                    catch (Exception)
                    {
                        pageExists = false;
                        break;
                    }
                } while (pageExists);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}
