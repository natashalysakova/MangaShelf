using MangaShelf.BL.Enums;
using MangaShelf.BL.Parsers;
using MangaShelf.Common.Interfaces;
using MangaShelf.Infrastructure.Network;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MangaShelf.Parser.Tests;

[TestClass]
[Ignore]
public class AmazonTestClass
{
    private AmazonParser _parser;

    [TestInitialize]
    public void Initialize()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        services.AddKeyedSingleton<IHtmlDownloader, AdvancedHtmlDownloader>(HtmlDownloaderKeys.Advanced);
        services.AddTransient<AmazonParser>();

        _parser = services.BuildServiceProvider().GetRequiredService<AmazonParser>();
    }

    [TestMethod]
    public async Task AmazonTest()
    {

        var result = await _parser.Parse("https://www.amazon.com/dp/B08FVLVXX6/");

        Assert.IsNotNull(result);
        Assert.AreEqual("Volume 1", result.title);
        Assert.AreEqual("Solo Leveling", result.series);
        Assert.AreEqual("DUBU", result.authors);
        Assert.AreEqual(1, result.volumeNumber);
        Assert.AreEqual("https://m.media-amazon.com/images/I/81y9XvteVOL._SL1500_.jpg", result.cover);
        Assert.AreEqual(DateTime.Parse("2021-03-02"), result.release);
        Assert.AreEqual("Yen Press", result.publisher);
        Assert.AreEqual("Digital", result.type);
        Assert.AreEqual("B08FVLVXX6", result.isbn);
        Assert.AreEqual(-1, result.totalVolumes);
        Assert.AreEqual(null, result.seriesStatus);
        Assert.AreEqual(null, result.originalSeriesName);
    }

    [TestMethod]
    public async Task AmazonPreorderTest()
    {
        var result = await _parser.Parse("https://www.amazon.com/gp/product/B0D7Z8TGNQ");

        Assert.IsNotNull(result);
        Assert.AreEqual("Volume 8", result.title);
        Assert.AreEqual("[Oshi no Ko]", result.series);
        Assert.AreEqual("Aka Akasaka,Mengo Yokoyari", result.authors);
        Assert.AreEqual(8, result.volumeNumber);
        Assert.AreEqual(string.Empty, result.cover);
        Assert.AreEqual(DateTime.Parse("2024-11-19"), result.release);
        Assert.AreEqual("Yen Press", result.publisher);
        Assert.AreEqual("Digital", result.type);
        Assert.AreEqual("B0D7Z8TGNQ", result.isbn);
        Assert.AreEqual(-1, result.totalVolumes);
        Assert.AreEqual(null, result.seriesStatus);
        Assert.AreEqual(null, result.originalSeriesName);

    }

    [TestMethod]
    public async Task AmazonOneShotTest()
    {
        var result = await _parser.Parse("https://www.amazon.com/dp/B01N0LT06V");

        Assert.IsNotNull(result);
        Assert.AreEqual("Nijigahara Holograph", result.title);
        Assert.AreEqual("Nijigahara Holograph", result.series);
        Assert.AreEqual("Inio Asano", result.authors);
        Assert.AreEqual(-1, result.volumeNumber);
        Assert.AreEqual("https://m.media-amazon.com/images/I/91lxpgZLQOL._SL1500_.jpg", result.cover);
        Assert.AreEqual(DateTime.Parse("2014-03-19"), result.release);
        Assert.AreEqual("Fantagraphics", result.publisher);
        Assert.AreEqual("Digital", result.type);
        Assert.AreEqual("B01N0LT06V", result.isbn);
        Assert.AreEqual(-1, result.totalVolumes);
        Assert.AreEqual(null, result.seriesStatus);
        Assert.AreEqual(null, result.originalSeriesName);
    }
}
