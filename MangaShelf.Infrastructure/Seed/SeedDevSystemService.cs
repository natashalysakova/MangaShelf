using MangaShelf.DAL.System;
using MangaShelf.DAL.System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MangaShelf.Infrastructure.Seed;

public class SeedDevSystemService : ISeedDataService
{
    private readonly ILogger<SeedDevSystemService> _logger;
    private readonly IDbContextFactory<MangaSystemDbContext> _dbContextFactory;

    public SeedDevSystemService(ILogger<SeedDevSystemService> logger, IDbContextFactory<MangaSystemDbContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }
    public string ActivitySourceName => "Seed dev system";
    public int Priority => 92;


    public async Task Run(CancellationToken token)
    {
        await FixTypes();
        await RemoveDownloaders();
    }

    private async Task RemoveDownloaders()
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        context.RemoveRange(context.Settings.Where(x => x.Section == "HtmlDownloaders"));
        await context.SaveChangesAsync();
    }

    private async Task FixTypes()
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        foreach (var item in DefaultSettings.Settings)
        {
            var existing = await context.Settings.SingleOrDefaultAsync(x => x.Section == item.Section && x.Key == item.Key);
            if (existing != null && existing.Type == SettingType.Unknown)
            {
                existing.Type = item.type;
                _logger.LogInformation("Updated setting {Section}:{Key} type to {Type}", item.Section, item.Key, item.type);
            }
        }

        await context.SaveChangesAsync();
    }
}
