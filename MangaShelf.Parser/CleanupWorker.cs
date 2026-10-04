using MangaShelf.BL.Configuration;
using MangaShelf.BL.Contracts;
using MangaShelf.BL.Services.Parsing;
using MangaShelf.Parser.Services;

namespace MangaShelf.Parser;

public class CleanupWorker(ILogger<CleanupWorker> logger, IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var options = scope.ServiceProvider.GetRequiredService<IConfigurationService>().Cleanup;

                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.LogTrace("CleanupWorker running at: {time}", DateTimeOffset.Now);
                }


                if (!options.EnableJobCleanup)
                {
                    await Task.Delay(options.CleanupInterval, stoppingToken);
                    continue;
                }

                var cutOffDate = DateTimeOffset.UtcNow.AddDays(-options.CleanupOldJobsAfterDays);
                var removeFailedJobs = options.CleanupFailedJobs;

                try
                {
                    var parseJobManagerService = scope.ServiceProvider.GetRequiredService<IParseJobManagerService>();
                    var deleted = await parseJobManagerService.DeleteOldJobs(cutOffDate, removeFailedJobs, stoppingToken);
                    logger.LogInformation("CleanupWorker deleted {DeletedCount} old jobs", deleted);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred while cleaning up old jobs.");
                }

                await Task.Delay(options.CleanupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("CleanupWorker stopping due to cancellation request");
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during cleanup");
            }

        }
    }
}
