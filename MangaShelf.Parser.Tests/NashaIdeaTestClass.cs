using MangaShelf.BL.Parsers;

namespace MangaShelf.Parser.Tests;

[TestClass]
public class NashaIdeaTestClass : BaseParserTestClass<NashaIdeaParser>
{

    [TestMethod]
    public async Task NashaIdea_PreorderTest()
    {
        var result = await Parser.Parse("https://nashaidea.com/product/ne-khlopets-tom-1/");

        Assert.IsNotNull(result);
        Assert.AreEqual("Том 1", result.title);
        Assert.AreEqual("Хлопець, який їй сподобався, — не хлопець", result.series);
        Assert.AreEqual("", result.authors);
        Assert.AreEqual(1, result.volumeNumber);
        Assert.IsNotNull(result.cover);
        Assert.IsTrue(result.cover.StartsWith("https://nashaidea.com/"));
        Assert.IsTrue(result.cover.EndsWith(".webp"));
        Assert.AreEqual("Nasha Idea", result.publisher);
        Assert.AreEqual("Physical", result.type);
        Assert.AreEqual("978-617-8516-51-2", result.isbn);
        Assert.AreEqual(3, result.totalVolumes);
        Assert.AreEqual("ongoing", result.seriesStatus);
        Assert.AreEqual("https://nashaidea.com/product/ne-khlopets-tom-1/", result.url);
        Assert.AreEqual(DateTime.Parse("2025-08-28"), result.preorderStartDate);
        Assert.AreEqual(DateTime.Parse("2025-09-30"), result.release);
        Assert.AreEqual(true, result.isPreorder);
    }

    [TestMethod]
    public async Task NashaIdea_RegularVolume()
    {
        var result = await Parser.Parse("https://nashaidea.com/product/chi-tom-10/");

        Assert.IsNotNull(result);
        Assert.AreEqual("Том 10", result.title);
        Assert.AreEqual("Чi “Життя однієї киці”", result.series);
        Assert.AreEqual("", result.authors);
        Assert.AreEqual(10, result.volumeNumber);
        Assert.AreEqual("https://nashaidea.com/wp-content/uploads/2024/07/chi10-x1080.webp", result.cover);
        Assert.AreEqual(null, result.release);
        Assert.AreEqual("Nasha Idea", result.publisher);
        Assert.AreEqual("Physical", result.type);
        Assert.AreEqual("978-617-8396-46-6", result.isbn);
        Assert.AreEqual(12, result.totalVolumes);
        Assert.AreEqual("finished", result.seriesStatus);
        Assert.AreEqual("https://nashaidea.com/product/chi-tom-10/", result.url);
        Assert.AreEqual(DateTime.Parse("2024-07-31"), result.preorderStartDate);
        Assert.AreEqual(null, result.release);
        Assert.AreEqual(false, result.isPreorder);

    }

    [TestMethod]
    public async Task NashaIdea_OneShotVolume()
    {
        var result = await Parser.Parse("https://nashaidea.com/product/vitaiemo-v-koto-kafe/");

        Assert.IsNotNull(result);
        Assert.AreEqual("Вітаємо в кото-кафе", result.title);
        Assert.AreEqual("Вітаємо в кото-кафе", result.series);
        Assert.AreEqual("", result.authors);
        Assert.AreEqual(-1, result.volumeNumber);
        Assert.AreEqual("https://nashaidea.com/wp-content/uploads/2023/10/cat-flat.webp", result.cover);
        Assert.AreEqual(null, result.release);
        Assert.AreEqual("Nasha Idea", result.publisher);
        Assert.AreEqual("Physical", result.type);
        Assert.AreEqual("978-617-8109-88-2", result.isbn);
        Assert.AreEqual(1, result.totalVolumes);
        Assert.AreEqual("oneshot", result.seriesStatus);
        Assert.AreEqual(false, result.isPreorder);

    }


    [TestMethod]
    public async Task NashaIdea_FinishedSeries()
    {
        var result = await Parser.Parse("https://nashaidea.com/product/proshhavaj-troyandovyj-sade-tom-3/");

        Assert.IsNotNull(result);
        Assert.AreEqual("Том 3", result.title);
        Assert.AreEqual("Прощавай, трояндовий саде", result.series);
        Assert.AreEqual("", result.authors);
        Assert.AreEqual(3, result.volumeNumber);
        Assert.AreEqual("https://nashaidea.com/wp-content/uploads/2024/08/mrg03-x1080.webp", result.cover);
        Assert.AreEqual(null, result.release);
        Assert.AreEqual("Nasha Idea", result.publisher);
        Assert.AreEqual("Physical", result.type);
        Assert.AreEqual("978-617-8396-55-8", result.isbn);
        Assert.AreEqual(3, result.totalVolumes);
        Assert.AreEqual("finished", result.seriesStatus);
        Assert.AreEqual(false, result.isPreorder);
        Assert.AreEqual(12, result.ageRestrictions);

    }

    [TestMethod]
    public async Task NashaIdea_PreorderDate_SholdBe_LastDayOfMonth()
    {
        var result = await Parser.Parse("https://nashaidea.com/product/given-1/");

        Assert.IsNotNull(result);
        Assert.AreEqual(DateTime.Parse("2025-07-09"), result.preorderStartDate);
        Assert.AreEqual(DateTime.Parse("2025-09-30"), result.release);
        Assert.AreEqual(true, result.isPreorder);
    }

    [TestMethod]
    public async Task NashaIdea_PublishDate_SholdBe()
    {
        var result = await Parser.Parse("https://nashaidea.com/product/hiraiasumi-tom-1/");

        Assert.IsNotNull(result);
        Assert.AreEqual(DateTime.Parse("2025-04-01"), result.preorderStartDate);
        Assert.AreEqual(null, result.release);
        Assert.AreEqual(false, result.isPreorder);
    }


    [TestMethod]
    public async Task NashaIdea_VolumeNumber_ShouldBe()
    {
        var result = await Parser.Parse("https://nashaidea.com/product/hirayasumi-tom-2/");

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.volumeNumber);
    }

    [TestMethod]
    public async Task NashaIdea_VolumeNumberAndTitle_ShouldBe()
    {
        var result = await Parser.Parse("https://nashaidea.com/product/zhoze-tygr-ta-ryba-tom-2/");

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.volumeNumber);
        Assert.AreEqual("Жозе, тигр та риба", result.series);
        Assert.AreEqual("том 2", result.title);
    }
}
