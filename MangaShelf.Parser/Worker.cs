using MangaShelf.BL.Contracts;

namespace MangaShelf.Parser;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IParseJobRunner _parseJobRunner;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IParseJobRunner parseJobRunner)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _parseJobRunner = parseJobRunner;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();

        try
        {
            await _parseJobRunner.InitializeParsers(stoppingToken);
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

            await _parseJobRunner.RunJobs(stoppingToken);

            await Task.Delay(options.LoopDelay, stoppingToken);
        }
    }
}
