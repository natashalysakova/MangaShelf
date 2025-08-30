using MangaShelf.BL.Enums;
using MangaShelf.BL.Parsers;
using MangaShelf.Common.Interfaces;
using MangaShelf.Infrastructure.Network;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MangaShelf.Parser.Tests;

[TestClass]
[Ignore]
public class KoboTestClass
{
    private KoboParser _parser;

    [TestInitialize]
    public void Initialize()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        services.AddKeyedSingleton<IHtmlDownloader, BasicHtmlDownloader>(HtmlDownloaderKeys.Basic);
        services.AddTransient<KoboParser>();

        _parser = services.BuildServiceProvider().GetRequiredService<KoboParser>();
    }

    [TestMethod]
    public async Task KoboTest()
    {

        var result = await _parser.Parse("https://www.kobo.com/ww/en/ebook/pretty-guardian-sailor-moon-eternal-edition-9");

        Assert.IsNotNull(result);
        Assert.AreEqual("Volume 9", result.title);
        Assert.AreEqual("Pretty Guardian Sailor Moon Eternal Edition", result.series);
        Assert.AreEqual("Naoko Takeuchi", result.authors);
        Assert.AreEqual(9, result.volumeNumber);
        Assert.AreEqual("https://cdn.kobo.com/book-images/b0ac3524-461c-4af5-bb05-d0f799ad87a1/353/569/90/False/pretty-guardian-sailor-moon-eternal-edition-9.jpg", result.cover);
        Assert.AreEqual(DateTime.Parse("2020-11-17"), result.release);
        Assert.AreEqual("Kodansha Comics", result.publisher);
        Assert.AreEqual("Digital", result.type);
        Assert.AreEqual("9781646595792", result.isbn);
        Assert.AreEqual(-1, result.totalVolumes);
        Assert.AreEqual(null, result.seriesStatus);
        Assert.AreEqual(null, result.originalSeriesName);

    }

    [TestMethod]
    public async Task KoboTest2()
    {
        var result = await _parser.Parse("https://www.kobo.com/ww/en/ebook/spy-x-family-family-portrait");

        Assert.IsNotNull(result);
        Assert.AreEqual("Spy x Family: Family Portrait", result.title);
        Assert.AreEqual("Spy x Family Novels", result.series);
        Assert.AreEqual("Aya Yajima", result.authors);
        Assert.AreEqual(-1, result.volumeNumber);
        Assert.AreEqual("https://cdn.kobo.com/book-images/a28d0839-b608-49ab-9f79-e100fef90c0f/353/569/90/False/spy-x-family-family-portrait.jpg", result.cover);
        Assert.AreEqual(DateTime.Parse("2023-12-26"), result.release);

        Assert.AreEqual("VIZ Media", result.publisher);
        Assert.AreEqual("Digital", result.type);

        Assert.AreEqual("9781974742691", result.isbn);
        Assert.AreEqual(-1, result.totalVolumes);
        Assert.AreEqual(null, result.seriesStatus);
        Assert.AreEqual(null, result.originalSeriesName);


    }

    [TestMethod]
    public async Task KoboPreorderTest()
    {
        var result = await _parser.Parse("https://www.kobo.com/ww/en/ebook/spy-x-family-vol-13");

        Assert.IsNotNull(result);
        Assert.AreEqual("Volume 13", result.title);
        Assert.AreEqual("Spy x Family", result.series);
        Assert.AreEqual("Tatsuya Endo", result.authors);
        Assert.AreEqual(13, result.volumeNumber);
        Assert.AreEqual("https://cdn.kobo.com/book-images/Images/00000000-0000-0000-0000-000000000000/353/569/90/False/empty_book_cover.jpg", result.cover);
        Assert.AreEqual(DateTime.Parse("2025-01-14"), result.release);
        Assert.AreEqual("VIZ Media", result.publisher);
        Assert.AreEqual("Digital", result.type);

        Assert.AreEqual("9781974753246", result.isbn);
        Assert.AreEqual(-1, result.totalVolumes);
        Assert.AreEqual(null, result.seriesStatus);
        Assert.AreEqual(null, result.originalSeriesName);

    }

    [TestMethod]
    public async Task KoboOneShotTest()
    {
        var result = await _parser.Parse("https://www.kobo.com/ww/en/ebook/a-girl-on-the-shore");

        Assert.IsNotNull(result);
        Assert.AreEqual("A Girl on the Shore", result.title);
        Assert.AreEqual("A Girl on the Shore", result.series);
        Assert.AreEqual("Inio Asano", result.authors);
        Assert.AreEqual(-1, result.volumeNumber);
        Assert.AreEqual("https://cdn.kobo.com/book-images/be122b39-d133-4162-8b54-bcca043d07bb/353/569/90/False/a-girl-on-the-shore.jpg", result.cover);
        Assert.AreEqual(DateTime.Parse("2016-01-19"), result.release);
        Assert.AreEqual("Kodansha USA", result.publisher);
        Assert.AreEqual("9781942993766", result.isbn);
        Assert.AreEqual(-1, result.totalVolumes);
        Assert.AreEqual(null, result.seriesStatus);

        Assert.AreEqual("Digital", result.type);

        Assert.AreEqual(null, result.originalSeriesName);

    }
}
