using MangaShelf.BL.Parsers;

namespace MangaShelf.Parser.Tests;

[TestClass]
public class LantsutaTestClass : BaseParserTestClass<LantsutaParser>
{

    [TestMethod]
    public async Task LantsutaTest()
    {
        var result = await Parser.Parse("https://lantsuta-publishing.com/seriy/series-apothecary-ua/apothecary3-ua");

        Assert.IsNotNull(result);
        Assert.AreEqual("Том 3", result.title);
        Assert.AreEqual("Монолог Травниці", result.series);
        Assert.AreEqual("Нацу Хюуґа", result.authors);
        Assert.AreEqual(3, result.volumeNumber);
        Assert.AreEqual("https://lantsuta-publishing.com/image/cache/catalog/Product/Apothecary/Kusuriya%20no%20hitorigoto-3_jacket-UA-425x650.jpg", result.cover);
        Assert.AreEqual(DateTime.Parse("2024-12-31"), result.release);
        Assert.AreEqual("LANTSUTA", result.publisher);
        Assert.AreEqual("Physical", result.type);
        Assert.AreEqual("978-617-8202-19-4", result.isbn);
        Assert.AreEqual(0, result.totalVolumes);
        Assert.AreEqual(null, result.seriesStatus);
        Assert.AreEqual(null, result.originalSeriesName);
        Assert.AreEqual(false, result.isPreorder);
        Assert.AreEqual(12, result.ageRestrictions);
    }

    [TestMethod]
    public async Task LantsutaPreorderTest()
    {

        var result = await Parser.Parse("https://lantsuta-publishing.com/manga-ua/apothecary11-ua");

        Assert.IsNotNull(result);
        Assert.AreEqual("Том 11", result.title);
        Assert.AreEqual("Монолог Травниці", result.series);
        Assert.AreEqual("Нацу Хюуґа", result.authors);
        Assert.AreEqual(11, result.volumeNumber);
        Assert.AreEqual("https://lantsuta-publishing.com/image/cache/catalog/Product/Apothecary/kusuriya-11_+-425x650.jpg", result.cover);
        Assert.AreEqual(DateTime.Parse("2025-11-30"), result.release);
        Assert.AreEqual("LANTSUTA", result.publisher);
        Assert.AreEqual("Physical", result.type);
        Assert.AreEqual("978-617-8202-48-4", result.isbn);
        Assert.AreEqual(0, result.totalVolumes);
        Assert.AreEqual(null, result.seriesStatus);
        Assert.AreEqual(null, result.originalSeriesName);
        Assert.AreEqual(null, result.preorderStartDate);
        Assert.AreEqual("https://lantsuta-publishing.com/manga-ua/apothecary11-ua", result.url);
        Assert.AreEqual(true, result.isPreorder);

    }

    [TestMethod]
    public async Task Lantsuta_Title_ShouldBeParsed()
    {
        var result = await Parser.Parse("https://lantsuta-publishing.com/manga-ua/avatar_tla_ua");

        Assert.IsNotNull(result);
        Assert.AreEqual("Обіцянка", result.title);
        Assert.AreEqual("Аватар. Останній Захисник", result.series);
        Assert.AreEqual(-1, result.volumeNumber);
    }

    [TestMethod]
    [DataRow("Видання на стадії виробництва і з'явиться у продажі\r\nу серпні 2025 року.", "2025-08-31")]
    [DataRow("Видання на стадії виробництва і з'явиться у продажі наприкінці 2025 року.", "2025-12-31")]
    [DataRow("Відправка з 20 серпня 2025 року.", "2025-08-20")]
    public async Task Lantsuta_Description_ShouldBe_ParsedToReleaseDate(string input, string dateTime)
    {
        var expectedDate = DateTime.SpecifyKind(DateTime.Parse(dateTime), DateTimeKind.Local);

        var result = LantsutaParser.ParseDescription(input);

        Assert.IsNotNull(result);
        Assert.AreEqual(expectedDate, result);
    }
}
