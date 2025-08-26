using MangaShelf.Parser.VolumeParsers;

namespace MangaShelf.Parser.Tests;

[TestClass]
public class AmazonTestClass
{
    [TestMethod]
    public async Task AmazonTest()
    {
        var url = "https://www.amazon.com/dp/B08FVLVXX6/";
        var parser = new PublisherParsersFactory().CreateParser(url);

        Assert.IsNotNull(parser);

        var result = await parser.Parse(url);

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
        var parser = new PublisherParsersFactory().CreateParser("https://www.amazon.com/gp/product/B0D7Z8TGNQ");

        Assert.IsNotNull(parser);

        var result = await parser.Parse();

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
        var parser = new PublisherParsersFactory().CreateParser("https://www.amazon.com/dp/B01N0LT06V");

        Assert.IsNotNull(parser);

        var result = await parser.Parse();

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
