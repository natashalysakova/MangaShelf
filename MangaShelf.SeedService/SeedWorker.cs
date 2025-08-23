using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MangaShelf.SeedService;

public class SeedWorker(ILogger<SeedWorker> logger, IServiceProvider serviceProvider, IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        foreach (var service in scope.ServiceProvider.GetServices<ISeedDataService>().OrderBy(x=>x.Priority))
        {
            using var activity = new Activity(service.ActivitySourceName).Start();
            try
            {
                logger.LogInformation("{Activity} started", service.ActivitySourceName);
                await service.Run(scope.ServiceProvider, stoppingToken);
                logger.LogInformation("{Activity} finished", service.ActivitySourceName);
            }
            catch (Exception ex)
            {
                activity?.AddException(ex);
                activity?.SetStatus(ActivityStatusCode.Error);
                logger.LogError(ex, "Error during seeding: {ErrorMessage}", ex.Message);
                logger.LogInformation("Error exist");
                Environment.Exit(-1);
            }
        }
        logger.LogInformation("Seeding done. Host stopped");
        hostApplicationLifetime.StopApplication();
        Environment.Exit(0);
    }
}
