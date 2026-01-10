using MangaShelf.BL.Enums;
using MangaShelf.BL.Interfaces;
using MangaShelf.Common.Interfaces;
using MangaShelf.Infrastructure.Network;
using Microsoft.Extensions.Configuration;
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
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"HtmlDownloaders:MaxRetries", "5"}
            })
            .AddJsonFile("appsettings.json")
            .Build();

        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(configuration);

        services
            .Configure<HtmlDownloadOptions>(
                configuration
                .GetSection(HtmlDownloadOptions.SectionName));

        services.AddLogging(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

        services.AddScoped<IHtmlDownloader, BasicHtmlDownloader>();
        services.AddKeyedScoped<IHtmlDownloader, BasicHtmlDownloader>(HtmlDownloaderKeys.Basic);
        services.AddKeyedScoped<IHtmlDownloader, AdvancedHtmlDownloader>(HtmlDownloaderKeys.Advanced);
        services.AddKeyedScoped<IHtmlDownloader, MalopusHtmlDownloader>(HtmlDownloaderKeys.Malopus);

        services.AddScoped<T>();

        Parser = services.BuildServiceProvider().GetRequiredService<T>();
    }
}