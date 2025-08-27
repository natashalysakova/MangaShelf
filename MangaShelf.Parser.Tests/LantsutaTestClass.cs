using MangaShelf.BL.Parsers;
using Microsoft.Extensions.Logging;

namespace MangaShelf.Parser.Tests;

[TestClass]
public class LantsutaTestClass
{
    ILoggerFactory loggerFactory = new LoggerFactory();

    [TestMethod]
    public async Task LantsutaTest()
    {
        var parser = new LantsutaParser(loggerFactory.CreateLogger<LantsutaParser>());

        var result = await parser.Parse("https://lantsuta-publishing.com/seriy/series-apothecary-ua/apothecary3-ua");

        Assert.IsNotNull(result);
        Assert.AreEqual("Том 3", result.title);
        Assert.AreEqual("Монолог Травниці", result.series);
        Assert.AreEqual("Нацу Хюуґа", result.authors);
        Assert.AreEqual(3, result.volumeNumber);
        Assert.AreEqual("https://lantsuta-publishing.com/image/cache/catalog/Product/Apothecary/Kusuriya%20no%20hitorigoto-3_jacket-UA-425x650.jpg", result.cover);
        Assert.AreEqual(null, result.release);
        Assert.AreEqual("LANTSUTA", result.publisher);
        Assert.AreEqual("Physical", result.type);
        Assert.AreEqual("978-617-8202-19-4", result.isbn);
        Assert.AreEqual(0, result.totalVolumes);
        Assert.AreEqual(null, result.seriesStatus);
        Assert.AreEqual(null, result.originalSeriesName);
        Assert.AreEqual(false, result.isPreorder);
    }

    [TestMethod]
    public async Task LantsutaPreorderTest()
    {
        var parser = new LantsutaParser(loggerFactory.CreateLogger<LantsutaParser>());

        var result = await parser.Parse("https://lantsuta-publishing.com/manga-ua/apothecary11-ua");

        Assert.IsNotNull(result);
        Assert.AreEqual("Том 11", result.title);
        Assert.AreEqual("Монолог Травниці", result.series);
        Assert.AreEqual("Нацу Хюуґа", result.authors);
        Assert.AreEqual(11, result.volumeNumber);
        Assert.AreEqual("https://lantsuta-publishing.com/image/cache/catalog/Product/Apothecary/kusuriya-11_+-425x650.jpg", result.cover);
        Assert.AreEqual(null, result.release);
        Assert.AreEqual("LANTSUTA", result.publisher);
        Assert.AreEqual("Physical", result.type);
        Assert.AreEqual("978-617-8202-48-4", result.isbn);
        Assert.AreEqual(0, result.totalVolumes);
        Assert.AreEqual(null, result.seriesStatus);
        Assert.AreEqual(null, result.originalSeriesName);
        Assert.AreEqual(null, result.preorderStartDate);
        Assert.AreEqual("https://lantsuta-publishing.com/manga-ua/apothecary11-ua", result.url);
        Assert.AreEqual(null, result.release);
        Assert.AreEqual(true, result.isPreorder);

    }

    [TestMethod]
    public async Task Lantsuta_Title_ShouldBeParsed()
    {
        var parser = new LantsutaParser(loggerFactory.CreateLogger<LantsutaParser>());

        var result = await parser.Parse("https://lantsuta-publishing.com/manga-ua/avatar_tla_ua");

        Assert.IsNotNull(result);
        Assert.AreEqual("Обіцянка", result.title);
        Assert.AreEqual("Аватар. Останній Захисник", result.series);
        Assert.AreEqual(-1, result.volumeNumber);
    }
}
