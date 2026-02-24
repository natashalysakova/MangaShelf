using MangaShelf.BL.Parsers.NashaIdea;
using MangaShelf.DAL.Models;

namespace MangaShelf.Parser.Tests;

[TestClass]
public class NashaIdeaTestClass : BaseParserTestClass<NashaIdeaParser>
{

    [TestMethod]
    public async Task NashaIdea_PreorderTest()
    {
        var result = await Parser.Parse("https://nashaidea.com/product/100-bazhan-do-peretvorennya-na-zombi-tom-6/");

        Assert.IsNotNull(result);
        Assert.AreEqual("Том 6", result.Title);
        Assert.AreEqual("100 бажань до перетворення на зомбі", result.Series);
        Assert.IsNull(result.Authors);
        Assert.AreEqual(6, result.VolumeNumber);
        Assert.IsNotNull(result.Cover);
        Assert.IsTrue(result.Cover.StartsWith("https://nashaidea.com/"));
        Assert.IsTrue(result.Cover.EndsWith(".webp"));
        Assert.AreEqual("Nasha Idea", result.Publisher);
        Assert.AreEqual(VolumeType.Physical, result.VolumeType);
        //Assert.AreEqual("978-617-8516-51-2", result.Isbn);
        Assert.AreEqual(20, result.TotalVolumes);
        Assert.AreEqual(SeriesStatus.Ongoing, result.SeriesStatus);
        Assert.AreEqual("https://nashaidea.com/product/100-bazhan-do-peretvorennya-na-zombi-tom-6/", result.Url);
        Assert.AreEqual(DateTime.Parse("2026-01-07"), result.PreorderStartDate);
        Assert.AreEqual(DateTime.Parse("2026-04-10"), result.Release);
        Assert.AreEqual(true, result.IsPreorder);
    }

    [TestMethod]
    public async Task NashaIdea_RegularVolume()
    {
        var result = await Parser.Parse("https://nashaidea.com/product/chi-tom-10/");

        Assert.IsNotNull(result);
        Assert.AreEqual("Том 10", result.Title);
        Assert.AreEqual("Чi “Життя однієї киці”", result.Series);
        Assert.IsNull(result.Authors);
        Assert.AreEqual(10, result.VolumeNumber);
        Assert.AreEqual("https://nashaidea.com/wp-content/uploads/2024/07/chi10-x1080.webp", result.Cover);
        Assert.AreEqual(null, result.Release);
        Assert.AreEqual("Nasha Idea", result.Publisher);
        Assert.AreEqual(VolumeType.Physical, result.VolumeType);
        //Assert.AreEqual("978-617-8396-46-6", result.Isbn);
        Assert.AreEqual(12, result.TotalVolumes);
        Assert.AreEqual(SeriesStatus.Completed, result.SeriesStatus);
        Assert.AreEqual("https://nashaidea.com/product/chi-tom-10/", result.Url);
        Assert.AreEqual(DateTime.Parse("2024-07-31"), result.PreorderStartDate);
        Assert.AreEqual(null, result.Release);
        Assert.AreEqual(false, result.IsPreorder);
    }

    [TestMethod]
    public async Task NashaIdea_OneShotVolume()
    {
        var result = await Parser.Parse("https://nashaidea.com/product/vitaiemo-v-koto-kafe/");

        Assert.IsNotNull(result);
        Assert.AreEqual("Вітаємо в кото-кафе", result.Title);
        Assert.AreEqual("Вітаємо в кото-кафе", result.Series);
        Assert.IsNull(result.Authors);
        Assert.AreEqual(-1, result.VolumeNumber);
        Assert.AreEqual("https://nashaidea.com/wp-content/uploads/2023/10/cat-flat.webp", result.Cover);
        Assert.AreEqual(null, result.Release);
        Assert.AreEqual("Nasha Idea", result.Publisher);
        Assert.AreEqual(VolumeType.Physical, result.VolumeType);
        //Assert.AreEqual("978-617-8109-88-2", result.Isbn);
        Assert.AreEqual(1, result.TotalVolumes);
        Assert.AreEqual(SeriesStatus.OneShot, result.SeriesStatus);
        Assert.AreEqual(false, result.IsPreorder);
    }

    [TestMethod]
    public async Task NashaIdea_FinishedSeries()
    {
        var result = await Parser.Parse("https://nashaidea.com/product/proshhavaj-troyandovyj-sade-tom-3/");

        Assert.IsNotNull(result);
        Assert.AreEqual("Том 3", result.Title);
        Assert.AreEqual("Прощавай, трояндовий саде", result.Series);
        Assert.IsNull(result.Authors);
        Assert.AreEqual(3, result.VolumeNumber);
        Assert.AreEqual("https://nashaidea.com/wp-content/uploads/2024/08/mrg03-x1080.webp", result.Cover);
        Assert.AreEqual(null, result.Release);
        Assert.AreEqual("Nasha Idea", result.Publisher);
        Assert.AreEqual(VolumeType.Physical, result.VolumeType);
        //Assert.AreEqual("978-617-8396-55-8", result.Isbn);
        Assert.AreEqual(3, result.TotalVolumes);
        Assert.AreEqual(SeriesStatus.Completed, result.SeriesStatus);
        Assert.AreEqual(false, result.IsPreorder);
        Assert.AreEqual(12, result.AgeRestrictions);
    }

    [TestMethod]
    public async Task NashaIdea_PreorderDate_SholdBe_LastDayOfMonth()
    {
        var result = await Parser.Parse("https://web.archive.org/web/20250210143653/https://nashaidea.com/product/stalevyj-alhimik-tom-6/");

        Assert.IsNotNull(result);
        Assert.AreEqual(DateTime.Parse("2025-02-04"), result.PreorderStartDate);
        Assert.AreEqual(DateTime.Parse("2025-03-31"), result.Release);
        Assert.AreEqual(true, result.IsPreorder);
    }

    [TestMethod]
    public async Task NashaIdea_PublishDate_SholdBe()
    {
        var result = await Parser.Parse("https://nashaidea.com/product/hiraiasumi-tom-1/");

        Assert.IsNotNull(result);
        Assert.AreEqual(DateTime.Parse("2025-04-01"), result.PreorderStartDate);
        Assert.AreEqual(null, result.Release);
        Assert.AreEqual(false, result.IsPreorder);
    }

    [TestMethod]
    public async Task NashaIdea_VolumeNumber_ShouldBe()
    {
        var result = await Parser.Parse("https://nashaidea.com/product/hirayasumi-tom-2/");

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.VolumeNumber);
    }

    [TestMethod]
    public async Task NashaIdea_VolumeNumberAndTitle_ShouldBe()
    {
        var result = await Parser.Parse("https://nashaidea.com/product/zhoze-tygr-ta-ryba-tom-2/");

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.VolumeNumber);
        Assert.AreEqual("Жозе, тигр та риба", result.Series);
        Assert.AreEqual("том 2", result.Title);
        Assert.IsNotNull(result.Description);
    }
}
