using MangaShelf.BL.Enums;
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
            .Build();

        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(configuration);

        services.AddLogging(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

        services.AddKeyedScoped<IHtmlDownloader, BasicHtmlDownloader>(HtmlDownloaderKeys.Basic);
        services.AddKeyedScoped<IHtmlDownloader, AdvancedHtmlDownloader>(HtmlDownloaderKeys.Advanced);

        services.AddScoped<T>();

        Parser = services.BuildServiceProvider().GetRequiredService<T>();
    }
}