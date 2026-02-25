using System.Globalization;
using MangaShelf.BL.Configuration;
using MangaShelf.BL.Exceptions;
using MangaShelf.DAL.Interceptors;
using MangaShelf.DAL.System;
using MangaShelf.DAL.System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MangaShelf.Tests;

public class ConfigurationServiceTests : IDisposable
{
    private readonly DbContextOptions<MangaSystemDbContext> _options;
    private readonly IDbContextFactory<MangaSystemDbContext> _dbContextFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<ILogger<ConfigurationService>> _loggerMock;
    private readonly ConfigurationService _service;

    public ConfigurationServiceTests()
    {
        var databaseName = Guid.NewGuid().ToString();

        _options = new DbContextOptionsBuilder<MangaSystemDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .EnableSensitiveDataLogging()
            .AddInterceptors(new AuditInterceptor())
            .Options;

        _dbContextFactory = new TestDbContextFactory(_options);
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _loggerMock = new Mock<ILogger<ConfigurationService>>();

        _service = new ConfigurationService(_dbContextFactory, _loggerMock.Object, _memoryCache);
    }

    [Fact]
    public async Task BackgroundWorker_LoadsSettingsFromDatabase()
    {
        await SeedBackgroundWorkerSettingsAsync(enabled: true, startDelay: TimeSpan.FromSeconds(5), loopDelay: TimeSpan.FromMinutes(1));

        var settings = _service.BackgroundWorker;

        Assert.True(settings.Enabled);
        Assert.Equal(TimeSpan.FromSeconds(5), settings.StartDelay);
        Assert.Equal(TimeSpan.FromMinutes(1), settings.LoopDelay);
    }

    [Fact]
    public async Task BackgroundWorker_MissingSetting_ThrowsConfigurationMissingException()
    {
        await SeedBackgroundWorkerSettingsAsync(enabled: true, startDelay: TimeSpan.FromSeconds(5), loopDelay: null);

        Assert.Throws<ConfigurationMissingException>(() => _service.BackgroundWorker);
    }

    [Fact]
    public async Task UpdateSectionValueAsync_UpdatesValueAndInvalidatesCache()
    {
        await SeedBackgroundWorkerSettingsAsync(enabled: true, startDelay: TimeSpan.FromSeconds(5), loopDelay: TimeSpan.FromMinutes(1));

        var cachedSettings = _service.BackgroundWorker;
        Assert.True(cachedSettings.Enabled);

        using var context = CreateContext();
        var setting = await context.Settings.SingleAsync(x => x.Section == "BackgroundWorker" && x.Key == "Enabled");
        setting.Value = "false";
        await _service.UpdateSectionValueAsync(setting);

        var updatedSettings = _service.BackgroundWorker;

        Assert.False(updatedSettings.Enabled);

        using var context2 = CreateContext();
        var setting2 = await context2.Settings.SingleAsync(x => x.Section == "BackgroundWorker" && x.Key == "Enabled");
        Assert.Equal("false", setting2.Value);
    }

    public void Dispose()
    {
        using var context = CreateContext();
        context.Database.EnsureDeleted();
        _memoryCache.Dispose();
    }

    private MangaSystemDbContext CreateContext()
    {
        return new MangaSystemDbContext(_options);
    }

    private async Task SeedBackgroundWorkerSettingsAsync(bool enabled, TimeSpan startDelay, TimeSpan? loopDelay)
    {
        using var context = CreateContext();

        context.Settings.Add(new Settings
        {
            Section = "BackgroundWorker",
            Key = "Enabled",
            Value = enabled.ToString()
        });

        context.Settings.Add(new Settings
        {
            Section = "BackgroundWorker",
            Key = "StartDelay",
            Value = startDelay.ToString("c", CultureInfo.InvariantCulture)
        });

        if (loopDelay.HasValue)
        {
            context.Settings.Add(new Settings
            {
                Section = "BackgroundWorker",
                Key = "LoopDelay",
                Value = loopDelay.Value.ToString("c", CultureInfo.InvariantCulture)
            });
        }

        await context.SaveChangesAsync();
    }

    private class TestDbContextFactory : IDbContextFactory<MangaSystemDbContext>
    {
        private readonly DbContextOptions<MangaSystemDbContext> _options;

        public TestDbContextFactory(DbContextOptions<MangaSystemDbContext> options)
        {
            _options = options;
        }

        public MangaSystemDbContext CreateDbContext()
        {
            return new MangaSystemDbContext(_options);
        }
    }
}