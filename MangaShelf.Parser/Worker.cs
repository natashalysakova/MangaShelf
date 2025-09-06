using MangaShelf.BL.Interfaces;
using Microsoft.Extensions.Options;

namespace MangaShelf.Parser;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IParseJobManager _jobManager;
    private readonly BackgroundWorkerOptions _options;

    public Worker(ILogger<Worker> logger, IOptions<BackgroundWorkerOptions> options, IServiceProvider serviceProvider, IParseJobManager jobManager)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _jobManager = jobManager;
        _options = options.Value;

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogDebug("Background worker is disabled, exiting");
            return;
        }

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
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Worker running at: {time}", DateTimeOffset.Now);
            }

            if (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker stopping due to cancellation request");
                return;
            }

            await _jobManager.CreateScheduledJobs();

            await _jobManager.RunScheduledJobs();


            await Task.Delay(_options.LoopDelay, stoppingToken);
        }
    }
}
