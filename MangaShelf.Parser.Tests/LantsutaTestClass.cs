using MangaShelf.Parser.VolumeParsers;

namespace MangaShelf.Parser.Tests;

[TestClass]
public class LantsutaTestClass
{

    [TestMethod]
    public async Task LantsutaTest()
    {
        var parser = new PublisherParsersFactory().CreateParser("https://lantsuta-publishing.com/seriy/series-apothecary-ua/apothecary3-ua");

        Assert.IsNotNull(parser);

        var result = await parser.Parse();

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
    }

    [TestMethod]
    public async Task LantsutaPreorderTest()
    {
        var parser = new PublisherParsersFactory().CreateParser("https://lantsuta-publishing.com/apothecary7-ua");

        Assert.IsNotNull(parser);

        var result = await parser.Parse();

        Assert.IsNotNull(result);
        Assert.AreEqual("Том 7", result.title);
        Assert.AreEqual("Монолог Травниці", result.series);
        Assert.AreEqual("Нацу Хюуґа", result.authors);
        Assert.AreEqual(7, result.volumeNumber);
        Assert.AreEqual("https://lantsuta-publishing.com/image/cache/catalog/Product/Apothecary/kusuriya-7_jacket-Pantone3262-425x650.jpg", result.cover);
        Assert.AreEqual(null, result.release);
        Assert.AreEqual("LANTSUTA", result.publisher);
        Assert.AreEqual("Physical", result.type);
        Assert.AreEqual("978-617-8202-30-9", result.isbn);
        Assert.AreEqual(0, result.totalVolumes);
        Assert.AreEqual(null, result.seriesStatus);
        Assert.AreEqual(null, result.originalSeriesName);
    }
}
