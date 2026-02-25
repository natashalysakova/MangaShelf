using MangaShelf.DAL;
using MangaShelf.DAL.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MangaShelf.Infrastructure.Seed;

public class SeedDevShelfService : ISeedDataService
{
    private readonly ILogger<SeedDevShelfService> _logger;
    private readonly IDbContextFactory<MangaDbContext> _dbContextFactory;
    private readonly MangaIdentityDbContext _identityContext;

    public SeedDevShelfService(
        ILogger<SeedDevShelfService> logger,
        IDbContextFactory<MangaDbContext> dbContextFactory,
        MangaIdentityDbContext identityContext)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _identityContext = identityContext;
    }

    public string ActivitySourceName => "Seed dev shelf";

    public int Priority => 91;

    public async Task Run(CancellationToken cancellationToken)
    {
        await FixPublicIds();
        await FixMissingUserNames();
        await FixAvgRating();

        await ResetReleaseDate();
    }

    private async Task ResetReleaseDate()
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        await context.Volumes.ForEachAsync((v) =>
        {
            v.ReleaseDate = null;
            _logger.LogInformation("Reset release date for volume {VolumeId} - {VolumeTitle}", v.Id, v.Title);

        });

        await context.SaveChangesAsync();
        _logger.LogInformation("Finished resetting release dates for all volumes.");
    }

    private async Task FixAvgRating()
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        var volumes = context.Volumes.Include(x => x.Readers).Where(x => x.Readers.Any(y => y.Rating != null && y.Rating.Value != 0)).IgnoreQueryFilters();

        foreach (var volume in volumes)
        {
            volume.AvgRating = volume.Readers.Where(x => x.Rating != null && x.Rating.Value != 0).Average(x => x.Rating!.Value);
            _logger.LogInformation("Updated average rating for volume {VolumeId} - {VolumeTitle} to {AvgRating}", volume.Id, volume.Title, volume.AvgRating);
        }

        await context.SaveChangesAsync();
        _logger.LogInformation("Finished updating average ratings for all volumes.");
    }

    private async Task FixMissingUserNames()
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        foreach (var user in context.Users)
        {
            if (user.VisibleUsername == null)
            {
                var visibleName = _identityContext.Users.SingleOrDefault(x => x.Id == user.IdentityUserId)?.UserName;

                user.VisibleUsername = visibleName;
                _logger.LogInformation("Updated visible username for user {UserId} to {VisibleUsername}", user.Id, user.VisibleUsername);
            }
        }

        await context.SaveChangesAsync();
        _logger.LogInformation("Finished updating visible usernames for all users.");
    }

    private async Task FixPublicIds()
    {
        var context = _dbContextFactory.CreateDbContext();

        var seriesList = await context.Series
            .Where(s => string.IsNullOrEmpty(s.PublicId))
            .ToListAsync();

        foreach (var series in seriesList)
        {
            series.PublicId = Guid.NewGuid().ToString();
            _logger.LogInformation("Updated public ID for series {SeriesId} - {SeriesTitle} to {PublicId}", series.Id, series.Title, series.PublicId);
        }

        var volumeList = await context.Volumes
            .Where(v => string.IsNullOrEmpty(v.PublicId))
            .ToListAsync();

        foreach (var volume in volumeList)
        {
            volume.PublicId = Guid.NewGuid().ToString();
            _logger.LogInformation("Updated public ID for volume {VolumeId} - {VolumeTitle} to {PublicId}", volume.Id, volume.Title, volume.PublicId);
        }

        await context.SaveChangesAsync();
        _logger.LogInformation("Finished updating public IDs for all series and volumes.");
    }
}
