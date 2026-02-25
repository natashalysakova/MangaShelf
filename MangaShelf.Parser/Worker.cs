using MangaShelf.BL.Interfaces;

namespace MangaShelf.Parser;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IParseJobManager _jobManager;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IParseJobManager jobManager)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _jobManager = jobManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var parserFactory = scope.ServiceProvider.GetRequiredService<IParserFactory>();
        var parsers = parserFactory.GetParsers();

        _logger.LogInformation("Found {ParserCount} parsers", parsers.Count());

        try
        {
            await _jobManager.InitializeParser(parsers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot initialize parsers, exiting");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var options = scope.ServiceProvider.GetRequiredService<IConfigurationService>().BackgroundWorker;

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Worker running at: {time}", DateTimeOffset.Now);
            }

            if (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker stopping due to cancellation request");
                return;
            }

            if (options.Enabled)
            {
                await _jobManager.CreateScheduledJobs();
            }

            await _jobManager.RunScheduledJobs();

            await Task.Delay(options.LoopDelay, stoppingToken);
        }
    }
}
