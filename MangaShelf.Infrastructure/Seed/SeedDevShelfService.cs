using MangaShelf.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace MangaShelf.Infrastructure.Seed;

public class SeedDevShelfService : ISeedDataService
{
    private readonly IDbContextFactory<MangaDbContext> _dbContextFactory;

    public SeedDevShelfService(ILogger<SeedDevShelfService> logger, IDbContextFactory<MangaDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public string ActivitySourceName => "Seed dev shelf";

    public int Priority => 91;

    public async Task Run(CancellationToken cancellationToken)
    {
        await FixPublicIds();

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
