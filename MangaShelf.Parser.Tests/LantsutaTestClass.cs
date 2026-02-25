using MangaShelf.BL.Parsers;
using MangaShelf.DAL.Models;

namespace MangaShelf.Parser.Tests;

[TestClass]
public class LantsutaTestClass : BaseParserTestClass<LantsutaParser>
{

    [TestMethod]
    public async Task LantsutaTest()
    {
        var result = await Parser.Parse("https://lantsuta-publishing.com/seriy/series-apothecary-ua/apothecary3-ua");

        Assert.IsNotNull(result);
        Assert.AreEqual("Том 3", result.Title);
        Assert.AreEqual("Монолог Травниці", result.Series);
        Assert.AreEqual("Нацу Хюуґа", result.Authors);
        Assert.AreEqual(3, result.VolumeNumber);
        Assert.AreEqual("https://lantsuta-publishing.com/image/cache/catalog/Product/Apothecary/Kusuriya%20no%20hitorigoto-3_jacket-UA-425x650.jpg", result.Cover);
        Assert.AreEqual(DateTime.Parse("2024-12-31"), result.Release);
        Assert.AreEqual("LANTSUTA", result.Publisher);
        Assert.AreEqual(VolumeType.Physical, result.VolumeType);
        Assert.AreEqual("978-617-8202-19-4", result.Isbn);
        Assert.AreEqual(-1, result.TotalVolumes);
        Assert.AreEqual(SeriesStatus.Unknown, result.SeriesStatus);
        Assert.AreEqual(null, result.OriginalSeriesName);
        Assert.AreEqual(false, result.IsPreorder);
        Assert.AreEqual(12, result.AgeRestrictions);
    }

    [TestMethod]
    [TestCategory("Flacky")]
    public async Task LantsutaPreorderTestMultipleYears()
    {
        var result = await Parser.Parse("https://lantsuta-publishing.com/manga-ua/apothecary11-ua");

        Assert.IsNotNull(result);
        Assert.AreEqual(true, result.IsPreorder);
    }

    [TestMethod]
    public async Task Lantsuta_Title_ShouldBeParsed()
    {
        var result = await Parser.Parse("https://lantsuta-publishing.com/manga-ua/avatar_tla_ua");

        Assert.IsNotNull(result);
        Assert.AreEqual("Обіцянка", result.Title);
        Assert.AreEqual("Аватар. Останній Захисник", result.Series);
        Assert.AreEqual(-1, result.VolumeNumber);
        Assert.IsNotNull(result.Description);
    }

    [TestMethod]
    [DataRow("Видання на стадії виробництва і з'явиться у продажі\r\nу серпні 2025 року.", "2025-08-31")]
    [DataRow("Видання на стадії виробництва і з'явиться у продажі наприкінці 2025 року.", "2025-12-31")]
    [DataRow("Відправка з 20 серпня 2025 року.", "2025-08-20")]
    [DataRow("Видання на стадії виробництва і з'явиться у продажі\r\nвзимку 2025-2026 року.", "2026-02-28")]
    public async Task Lantsuta_Description_ShouldBe_ParsedToReleaseDate(string input, string dateTime)
    {
        var expectedDate = DateTime.SpecifyKind(DateTime.Parse(dateTime), DateTimeKind.Local);

        var result = LantsutaParser.ParseDescription(input);

        Assert.IsNotNull(result);
        Assert.AreEqual(expectedDate, result);
    }
}
