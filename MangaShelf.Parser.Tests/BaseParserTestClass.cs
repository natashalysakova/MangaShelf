using MangaShelf.BL.Configuration;
using MangaShelf.BL.Enums;
using MangaShelf.BL.Interfaces;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.System.Models;
using MangaShelf.Infrastructure.Network;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MangaShelf.Parser.Tests;

[TestClass]
public abstract class BaseParserTestClass<T> where T : class, IPublisherParser
{
    protected T Parser { get; set; }

    [TestInitialize]
    public void Initialize()
    {
        var services = new ServiceCollection();
        //var configuration = new ConfigurationBuilder()
        //    .AddInMemoryCollection(new Dictionary<string, string?>
        //    {
        //        ["HtmlDownloaders:MaxRetries"] = "5"
        //    })
        //    .Build();

        //services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IConfigurationService>(new TestConfigurationService());

        services.AddLogging(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

        services.AddScoped<IHtmlDownloader, BasicHtmlDownloader>();
        services.AddKeyedScoped<IHtmlDownloader, BasicHtmlDownloader>(HtmlDownloaderKeys.Basic);
        services.AddKeyedScoped<IHtmlDownloader, AdvancedHtmlDownloader>(HtmlDownloaderKeys.Advanced);
        services.AddKeyedScoped<IHtmlDownloader, MalopusHtmlDownloader>(HtmlDownloaderKeys.Malopus);

        services.AddScoped<T>();

        Parser = services.BuildServiceProvider().GetRequiredService<T>();
    }

    private sealed class TestConfigurationService : IConfigurationService
    {
        public BackgroundWorkerSettings BackgroundWorker => new()
        {
            Enabled = false,
            StartDelay = TimeSpan.Zero,
            LoopDelay = TimeSpan.Zero
        };

        public JobManagerSettings JobManager => new()
        {
            DelayBetweenRuns = TimeSpan.Zero,
            MaxParallelParsers = 1,
            ResetNextRun = false
        };

        public ParserServiceSettings ParserService => new()
        {
            DelayBetweenParse = TimeSpan.Zero,
            IgnoreExistingVolumes = false
        };

        public HtmlDownloaderSettings HtmlDownloader => new()
        {
            RequestTimeout = TimeSpan.FromSeconds(30),
            MaxRetries = 1,
            DelayBetweenRetries = TimeSpan.FromMilliseconds(100)
        };

        // Fix for CS0535: implement CacheSettings property
        public CacheSettings CacheSettings => new();

        public void InvalidateSection<TSection>() where TSection : class, BL.Interfaces.IConfigurationSection, new()
        {
        }

        public Task UpdateSectionValueAsync<TSection>(string key, string value, CancellationToken token = default)
            where TSection : class, BL.Interfaces.IConfigurationSection, new()
        {
            return Task.CompletedTask;
        }

        // Fix for CS0535: implement UpdateSectionValueAsync(Settings, CancellationToken)
        public Task<Settings> UpdateSectionValueAsync(Settings settings, CancellationToken token = default)
        {
            return Task.FromResult(settings);
        }
    }
}