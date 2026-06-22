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

    private async Task MakeSureDbCreatedAsync<T>(IServiceScope scope) where T : DbContext
    {
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<T>>();
        using var context = await factory.CreateDbContextAsync();
        if (context.Database.HasPendingModelChanges())
        {
            throw new InvalidOperationException($"System database model ({typeof(T).Name}) has pending changes. Please apply migrations before starting the application.");
        }

        logger.LogInformation("Applying migrations for {DbContextName}", typeof(T).Name);
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

    
}