using MangaShelf.DAL;
using MangaShelf.DAL.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;

namespace MangaShelf.Infrastructure.Seed;

public class SeedDevShelfService : ISeedDataService
{
    private readonly IDbContextFactory<MangaDbContext> _dbContextFactory;
    private readonly MangaIdentityDbContext _identityContext;

    public SeedDevShelfService(
        ILogger<SeedDevShelfService> logger, 
        IDbContextFactory<MangaDbContext> dbContextFactory,
        MangaIdentityDbContext identityContext)
    {
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

    }

    private async Task FixAvgRating()
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        var volumes = context.Volumes.Include(x => x.Readers).Where(x=>x.Readers.Any(y=>y.Rating!=null && y.Rating.Value != 0)).IgnoreQueryFilters();

        foreach (var volume in volumes)
        {
            volume.AvgRating = volume.Readers.Where(x => x.Rating != null && x.Rating.Value != 0).Average(x => x.Rating!.Value);
        }

        await context.SaveChangesAsync();
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
            }
        }

        await context.SaveChangesAsync();
    }

    public async Task Run()
    {
        await Run(CancellationToken.None);
    }

    private async Task FixPublicIds()
    {
        var context = _dbContextFactory.CreateDbContext();

        var seriesList = await context.Series
            .Where(s => s.PublicId == Guid.Empty)
            .ToListAsync();

        foreach (var series in seriesList)
        {
            series.PublicId = Guid.NewGuid();
        }

        var volumeList = await context.Volumes
            .Where(v => v.PublicId == Guid.Empty)
            .ToListAsync();

        foreach (var volume in volumeList)
        {
            volume.PublicId = Guid.NewGuid();
        }

        await context.SaveChangesAsync();
    }
}
