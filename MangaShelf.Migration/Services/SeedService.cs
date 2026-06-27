using MangaShelf.DAL;
using MangaShelf.DAL.Identity;
using MangaShelf.DAL.System;
using MangaShelf.Infrastructure.Seed;
using MangaShelf.Migration.DataCorrections;
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

            await ApplyDataCorrections();
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

    private async Task ApplyDataCorrections()
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IServiceProvider>>();
        
        var _dataCorrections = scope.ServiceProvider.GetServices<IDataCorrection>().OrderBy(x => x.GetType().Name).ToList();

        var _factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MangaDbContext>>();
        using var context = await _factory.CreateDbContextAsync();

        var systemContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MangaSystemDbContext>>();
        var systemContext = await systemContextFactory.CreateDbContextAsync();

        foreach (var correction in _dataCorrections)
        {
            var correctionName = correction.GetType().Name;
            var correctionResult = await systemContext.DataCorrections.FirstOrDefaultAsync(x => x.Name == correctionName);

            if(correctionResult != null)
            {
                logger.LogInformation($"Data correction {correctionName} already applied on {correctionResult.AppliedOn}");
                continue;
            }


            logger.LogInformation($"Applying data correction: {correction.GetType().Name}");
            await correction.ApplyCorrection(context);
            await context.SaveChangesAsync();

            systemContext.DataCorrections.Add(new DataCorrection { Name = correctionName });
            await systemContext.SaveChangesAsync();
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