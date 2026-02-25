using MangaShelf.DAL.System;
using MangaShelf.DAL.System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MangaShelf.Infrastructure.Seed;

public class SeedProdSystemService : ISeedDataService
{
    private readonly ILogger<SeedProdSystemService> _logger;
    private readonly IDbContextFactory<MangaSystemDbContext> _dbContextFactory;

    public SeedProdSystemService(ILogger<SeedProdSystemService> logger, IDbContextFactory<MangaSystemDbContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }
    public string ActivitySourceName => "Seed prod system";
    public int Priority => 3;


    public async Task Run(CancellationToken token)
    {
        await CreateSections();
    }
    private async Task CreateSections()
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        foreach (var item in DefaultSettings.Settings)
        {
            var existing = await context.Settings.SingleOrDefaultAsync(x => x.Section == item.Section && x.Key == item.Key);
            if (existing == null)
            {
                context.Settings.Add(new Settings
                {
                    Section = item.Section,
                    Key = item.Key,
                    Value = item.Value,
                    Type = item.type
                });
                _logger.LogInformation("Added setting {Section}:{Key} with value {Value}", item.Section, item.Key, item.Value);
            }
        }

        await context.SaveChangesAsync();
    }
}
