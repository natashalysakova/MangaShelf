using MangaShelf.DAL;
using MangaShelf.DAL.Identity;
using MangaShelf.DAL.System;
using MangaShelf.DAL.System.Models;
using MangaShelf.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;

public class SeedService(
    ILogger<SeedService> logger,
    IServiceProvider serviceProvider,
    IHostApplicationLifetime lifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await MakeSureDbCreatedAsync();
            await SeedDatabase();
            await ResetStuckJobs(serviceProvider);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error during migration: {ErrorMessage}", ex.Message);
            // Exit with non-zero code so Docker restarts and depends_on fails correctly
            Environment.Exit(1);
        }
        finally
        {
            // Signal the host to stop — this makes the container exit with code 0
            lifetime.StopApplication();
        }
    }

    public async Task SeedDatabase()
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IServiceProvider>>();
        logger.LogInformation("Seeding Started");
        foreach (var service in scope.ServiceProvider.GetServices<ISeedDataService>().OrderBy(x => x.Priority))
        {
            try
            {
                logger.LogInformation("{Activity} started", service.ActivitySourceName);
                await service.Run();
                logger.LogInformation("{Activity} finished", service.ActivitySourceName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during seeding: {ErrorMessage}", ex.Message);
            }
        }
        logger.LogInformation("Seeding done");
    }

    public async Task MakeSureDbCreatedAsync()
    {
        using (var scope = serviceProvider.CreateScope())
        {
            await MakeSureDbCreatedAsync<MangaSystemDbContext>(scope);
            await MakeSureDbCreatedAsync<MangaDbContext>(scope);
            await MakeSureIdentityDbCreatedAsync(scope);
        }
    }

    private static async Task MakeSureDbCreatedAsync<T>(IServiceScope scope) where T : DbContext
    {
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<T>>();
        using var context = await factory.CreateDbContextAsync();
        if (context.Database.HasPendingModelChanges())
        {
            throw new InvalidOperationException($"System database model ({typeof(T).Name}) has pending changes. Please apply migrations before starting the application.");
        }

        await context.Database.MigrateAsync();
    }

    private static async Task MakeSureIdentityDbCreatedAsync(IServiceScope scope)
    {
        using var context = scope.ServiceProvider.GetRequiredService<MangaIdentityDbContext>();
        if (context.Database.HasPendingModelChanges())
        {
            throw new InvalidOperationException($"System database model ({nameof(MangaIdentityDbContext)}) has pending changes. Please apply migrations before starting the application.");
        }

        await context.Database.MigrateAsync();
    }

    private  async Task ResetStuckJobs(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<MangaSystemDbContext>();

        try
        {
            var parserStatuses = context.Parsers
                .Include(p => p.Jobs)
                    .ThenInclude(r => r.Errors);

            var notFinishedProperly = parserStatuses
                .SelectMany(x => x.Jobs)
                .Where(r => r.Status == RunStatus.Waiting || r.Status == RunStatus.Running);

            foreach (var job in notFinishedProperly)
            {
                job.Status = RunStatus.Error;
                job.Finished = DateTimeOffset.Now;
                job.Progress = -1;
                job.Errors.Add(new ParserError()
                {
                    ErrorMessage = "Was automatically cancelled after restart",
                    RunTime = job.Finished.Value
                });
            }

            foreach (var parser in parserStatuses)
            {
                parser.Status = ParserStatus.Idle;
            }

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // do nothing, we don't want to block the app from starting
        }
    }
}