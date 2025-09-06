using MangaShelf.BL.Parsers.Malopus;
using MangaShelf.DAL.Models;

namespace MangaShelf.Parser.Tests;

[TestClass]
public class MalopusTestClass : BaseParserTestClass<MalopusParser>
{
    [TestMethod]
    public async Task MalopusTest()
    {
        var result = await Parser.Parse("https://malopus.com.ua/manga/manga-cya-porcelyanova-lyalechka-zakohalasya-tom-5");

        Assert.IsNotNull(result);
        Assert.AreEqual("Том 5", result.Title);
        Assert.AreEqual("Ця порцелянова лялечка закохалася", result.Series);
        Assert.AreEqual("Шін'ічі Фукуда", result.Authors);
        Assert.AreEqual(5, result.VolumeNumber);
        Assert.AreEqual("https://malopus.com.ua/image/cache/catalog/import_files/my%20dress%20up%20darling/005/Moc_Cover_ЦПЛЗ%205-700x700.png", result.Cover);
        Assert.AreEqual(null, result.Release);
        Assert.AreEqual("Mal'opus", result.Publisher);
        Assert.AreEqual(VolumeType.Physical, result.VolumeType);
        Assert.AreEqual("978-617-8168-12-4", result.Isbn);
        Assert.AreEqual(15, result.TotalVolumes);
        Assert.AreEqual(SeriesStatus.Completed, result.SeriesStatus);
        Assert.AreEqual("Sono Bisque Doll wa Koi wo Suru", result.OriginalSeriesName);
        Assert.AreEqual(false, result.IsPreorder);
        Assert.AreEqual(18, result.AgeRestrictions);
    }

    [TestMethod]
    public async Task MalopusPreorderTest()
    {
        var result = await Parser.Parse("https://malopus.com.ua/manga/kuroshitsuji-vol-6");

        Assert.IsNotNull(result);
        Assert.AreEqual("Том 6", result.Title);
        Assert.AreEqual("Темний дворецький", result.Series);
        Assert.AreEqual("Яна Тобосо", result.Authors);
        Assert.AreEqual(6, result.VolumeNumber);
        Assert.AreEqual("https://malopus.com.ua/image/cache/catalog/import_files/kuroshitsuji/006/Moc_Cover%20_Темний%20Дворецький_6-700x700.png", result.Cover);
        Assert.AreEqual(DateTime.Parse("2025-09-14"), result.Release);
        Assert.AreEqual("Mal'opus", result.Publisher);
        Assert.AreEqual(VolumeType.Physical, result.VolumeType);
        Assert.AreEqual("978-617-8168-68-1", result.Isbn);
        Assert.AreEqual(34, result.TotalVolumes);
        Assert.AreEqual(SeriesStatus.Ongoing, result.SeriesStatus);
        Assert.AreEqual("Kuroshitsuji", result.OriginalSeriesName);
        Assert.AreEqual(true, result.IsPreorder);
        Assert.AreEqual(DateTime.Parse("2025-07-28"), result.PreorderStartDate);
        Assert.AreEqual(null, result.AgeRestrictions);
        Assert.IsNotNull(result.Description);
    }

    [TestMethod]
    public async Task MalopusOneShotTest()
    {
        var result = await Parser.Parse("https://malopus.com.ua/manga/nijigahara-holograph");

        Assert.IsNotNull(result);
        Assert.AreEqual("Голограф Веселкового поля", result.Title);
        Assert.AreEqual("Голограф Веселкового поля", result.Series);
        Assert.AreEqual("Ініо Асано", result.Authors);
        Assert.AreEqual(-1, result.VolumeNumber);
        Assert.AreEqual("https://malopus.com.ua/image/cache/catalog/import_files/nijigahara/Moc_Cover_Голограф%20веселкового%20поля-700x700.png", result.Cover);
        Assert.AreEqual(null, result.Release);
        Assert.AreEqual("Mal'opus", result.Publisher);
        Assert.AreEqual(VolumeType.Physical, result.VolumeType);
        Assert.AreEqual("978-617-8168-11-7", result.Isbn);
        Assert.AreEqual(1, result.TotalVolumes);
        Assert.AreEqual(SeriesStatus.OneShot, result.SeriesStatus);
        Assert.AreEqual("Nijigahara Holograph", result.OriginalSeriesName);
        Assert.AreEqual(false, result.IsPreorder);
    }

    [TestMethod]
    public async Task Malopus_VolumeNumber_ShouldBe()
    {
        var result = await Parser.Parse("https://malopus.com.ua/manga/dark-souls-redemption-vol1");


        Assert.AreEqual(1, result.VolumeNumber);
        Assert.AreEqual(1, result.TotalVolumes);
        Assert.AreEqual(SeriesStatus.Ongoing, result.SeriesStatus);
    }

    [TestMethod]
    public async Task Malopus_ReleaseDate_ShouldBe_Parsed()
    {
        var result = await Parser.Parse("https://malopus.com.ua/manga/junji-ito-shiver");

        Assert.AreEqual(DateTime.Parse("2025-10-31"), result.Release);
    }

    [TestMethod]
    public async Task Malopus_TitleAndSeries_ShouldBe_Parsed()
    {
        var result = await Parser.Parse("https://malopus.com.ua/manga/bocchi-the-rock-vol2");

        Assert.AreEqual("Bocchi the Rock!", result.Series);
        Assert.AreEqual("Том 2", result.Title);
        Assert.AreEqual(2, result.VolumeNumber);
        Assert.AreEqual(DateTime.Parse("2024-06-17"), result.PreorderStartDate);
    }
}
