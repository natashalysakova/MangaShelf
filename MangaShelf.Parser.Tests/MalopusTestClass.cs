
using MangaShelf.BL.Parsers;
using Microsoft.Extensions.Logging;

namespace MangaShelf.Parser.Tests;

[TestClass]
public class MalopusTestClass
{
    ILoggerFactory loggerFactory = new LoggerFactory();

    [TestMethod]
    public async Task MalopusTest()
    {
        var parser = new MalopusParser(loggerFactory.CreateLogger<MalopusParser>());

        var result = await parser.Parse("https://malopus.com.ua/manga/manga-cya-porcelyanova-lyalechka-zakohalasya-tom-5");

        Assert.IsNotNull(result);
        Assert.AreEqual("Том 5", result.title);
        Assert.AreEqual("Ця порцелянова лялечка закохалася", result.series);
        Assert.AreEqual("Шін'ічі Фукуда", result.authors);
        Assert.AreEqual(5, result.volumeNumber);
        Assert.AreEqual("https://malopus.com.ua/image/cache/catalog/import_files/my%20dress%20up%20darling/005/Moc_Cover_ЦПЛЗ%205-700x700.png", result.cover);
        Assert.AreEqual(null, result.release);
        Assert.AreEqual("Mal'opus", result.publisher);
        Assert.AreEqual("Physical", result.type);
        Assert.AreEqual("978-617-8168-12-4", result.isbn);
        Assert.AreEqual(15, result.totalVolumes);
        Assert.AreEqual("finished", result.seriesStatus);
        Assert.AreEqual("Sono Bisque Doll wa Koi wo Suru", result.originalSeriesName);
        Assert.AreEqual(false, result.isPreorder);
    }

    [TestMethod]
    public async Task MalopusPreorderTest()
    {
        var parser = new MalopusParser(loggerFactory.CreateLogger<MalopusParser>());

        var result = await parser.Parse("https://malopus.com.ua/manga/kuroshitsuji-vol-6");

        Assert.IsNotNull(result);
        Assert.AreEqual("Том 6", result.title);
        Assert.AreEqual("Темний дворецький", result.series);
        Assert.AreEqual("Яна Тобосо", result.authors);
        Assert.AreEqual(6, result.volumeNumber);
        Assert.AreEqual("https://malopus.com.ua/image/cache/catalog/import_files/kuroshitsuji/006/Moc_Cover%20_Темний%20Дворецький_6-700x700.png", result.cover);
        Assert.AreEqual(DateTime.Parse("2025-08-31"), result.release);
        Assert.AreEqual("Mal'opus", result.publisher);
        Assert.AreEqual("Physical", result.type);
        Assert.AreEqual("978-617-8168-68-1", result.isbn);
        Assert.AreEqual(34, result.totalVolumes);
        Assert.AreEqual("ongoing", result.seriesStatus);
        Assert.AreEqual("Kuroshitsuji", result.originalSeriesName);
        Assert.AreEqual(true, result.isPreorder);

    }

    [TestMethod]
    public async Task MalopusOneShotTest()
    {
        var parser = new MalopusParser(loggerFactory.CreateLogger<MalopusParser>());

        var result = await parser.Parse("https://malopus.com.ua/manga/nijigahara-holograph");

        Assert.IsNotNull(result);
        Assert.AreEqual("Голограф Веселкового поля", result.title);
        Assert.AreEqual("Голограф Веселкового поля", result.series);
        Assert.AreEqual("Ініо Асано", result.authors);
        Assert.AreEqual(-1, result.volumeNumber);
        Assert.AreEqual("https://malopus.com.ua/image/cache/catalog/import_files/nijigahara/Moc_Cover_Голограф%20веселкового%20поля-700x700.png", result.cover);
        Assert.AreEqual(null, result.release);
        Assert.AreEqual("Mal'opus", result.publisher);
        Assert.AreEqual("Physical", result.type);
        Assert.AreEqual("978-617-8168-11-7", result.isbn);
        Assert.AreEqual(1, result.totalVolumes);
        Assert.AreEqual("oneshot", result.seriesStatus);
        Assert.AreEqual("Nijigahara Holograph", result.originalSeriesName);
        Assert.AreEqual(false, result.isPreorder);
    }

    [TestMethod]
    public async Task Malopus_VolumeNumber_ShouldBe()
    {
        var parser = new MalopusParser(loggerFactory.CreateLogger<MalopusParser>());

        var result = await parser.Parse("https://malopus.com.ua/manga/dark-souls-redemption-vol1");


        Assert.AreEqual(1, result.volumeNumber);
        Assert.AreEqual(1, result.totalVolumes);
        Assert.AreEqual("ongoing", result.seriesStatus);
    }

    [TestMethod]
    public async Task Malopus_ReleaseDate_ShouldBe_Parsed()
    {
        var parser = new MalopusParser(loggerFactory.CreateLogger<MalopusParser>());

        var result = await parser.Parse("https://malopus.com.ua/manga/junji-ito-shiver");

        Assert.AreEqual(DateTime.Parse("2025-10-31"), result.release);
    }
}
