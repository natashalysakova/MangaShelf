using MangaShelf.DAL.System;
using MangaShelf.DAL.System.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.BL.Services;

public class SettingReadService(IDbContextFactory<MangaSystemDbContext> contextFactory) : ISettingReadService
{
    public async Task<IEnumerable<Settings>> GetAll(CancellationToken token = default)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.Settings
            .AsNoTracking()
            .ToListAsync(token);
    }

    public async Task<IEnumerable<Settings>> GetAllFromSection(string section, CancellationToken token = default)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.Settings
            .AsNoTracking()
            .Where(s => s.Section == section)
            .ToListAsync(token);
    }

    public async Task<IDictionary<string, IEnumerable<Settings>>> GetAllGrouped(CancellationToken token = default)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.Settings
            .AsNoTracking()
            .GroupBy(s => s.Section)
            .ToDictionaryAsync(g => g.Key, g => g.AsEnumerable());
    }

    public async Task<Settings?> GetById(Guid id, CancellationToken token = default)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.Settings
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == id, token);
    }

    public async Task<Settings?> GetByKey(string section, string key, CancellationToken token = default)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.Settings
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Section == section && s.Key == key, token);
    }
}
