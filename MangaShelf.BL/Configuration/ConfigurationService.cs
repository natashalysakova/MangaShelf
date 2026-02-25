using MangaShelf.BL.Exceptions;
using MangaShelf.BL.Interfaces;
using MangaShelf.DAL.System;
using MangaShelf.DAL.System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Reflection;

namespace MangaShelf.BL.Configuration;

public class ConfigurationService(
    IDbContextFactory<MangaSystemDbContext> dbContextFactory,
    ILogger<ConfigurationService> logger,
    IMemoryCache cache) : IConfigurationService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromDays(30);

    public BackgroundWorkerSettings BackgroundWorker => GetSection<BackgroundWorkerSettings>();
    public JobManagerSettings JobManager => GetSection<JobManagerSettings>();
    public ParserServiceSettings ParserService => GetSection<ParserServiceSettings>();
    public HtmlDownloaderSettings HtmlDownloader => GetSection<HtmlDownloaderSettings>();
    public CacheSettings CacheSettings => GetSection<CacheSettings>();

    public async Task<Settings> UpdateSectionValueAsync(Settings settings, CancellationToken token = default)
    {
        await using var context = dbContextFactory.CreateDbContext();
        
        context.Settings.Update(settings);

        await context.SaveChangesAsync(token);

        InvalidateSection(settings.Section);

        return settings;
    }

    private void InvalidateSection(string section)
    {
        cache.Remove(GetCacheKey(section));
    }
    public void InvalidateSection<TSection>() where TSection : class, IConfigurationSection, new()
    {
        cache.Remove(GetCacheKey<TSection>());
    }

    private TSection GetSection<TSection>() where TSection : class, IConfigurationSection, new()
    {
        if(cache.TryGetValue(GetCacheKey<TSection>(), out TSection? section))
        {
            return section!;
        }
        else
        {
            var entry = cache.CreateEntry(GetCacheKey<TSection>());
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            var result = BindSettings<TSection>();
            entry.SetValue(result);
            return result;
        }

        return cache.GetOrCreate(GetCacheKey<TSection>(), entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return BindSettings<TSection>();
        })!;
    }

    private static string GetCacheKey<TSection>() where TSection : class, IConfigurationSection, new()
    {
        return GetCacheKey(GetSectionName<TSection>());
    }
    private static string GetCacheKey(string section) => $"settings:{section}";

    private static string GetSectionName<TSection>() where TSection : class, IConfigurationSection, new()
        => new TSection().Name;

    private T BindSettings<T>() where T : class, IConfigurationSection, new()
    {
        var type = typeof(T);
        var obj = Activator.CreateInstance(type);

        var result = obj as T;

        using var context = dbContextFactory.CreateDbContext();
        var settings = context.Settings.Where(x => x.Section == result.Name).ToList();

        foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (settings.Any(x => x.Key == property.Name))
            {
                var config = settings.Single(x => x.Key == property.Name);

                var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                if (targetType.IsEnum)
                {
                    property.SetValue(obj, Enum.ToObject(targetType, int.Parse(config.Value)));
                }
                else if (targetType == typeof(TimeSpan))
                {
                    property.SetValue(obj, TimeSpan.Parse(config.Value, CultureInfo.InvariantCulture));
                }
                else
                {
                    property.SetValue(obj, Convert.ChangeType(config.Value, targetType, CultureInfo.InvariantCulture));
                }
                logger.LogTrace("Configuration loaded: {Type} {Property} = {Value}", type.Name, property.Name, config.Value);
            }
            else
            {
                logger.LogError("Configuration missing: {Type} {Property}", type.Name, property.Name);
                throw new ConfigurationMissingException($"{type.Name} {property.Name} is missing");
            }
        }

        return (T)obj!;
    }
}
