using MangaShelf.Parser.VolumeParsers;

namespace MangaShelf.Parser.Tests;

[TestClass]
public class NashaIdeaTestClass
{

    [TestMethod]
    public async Task NashaIdea_PreorderTest()
    {


        var parser = new PublisherParsersFactory().CreateParser("https://nashaidea.com/product/ne-khlopets-tom-1/");

        Assert.IsNotNull(parser);

        var result = await parser.Parse();

        Assert.IsNotNull(result);
        Assert.AreEqual("Том 1", result.title);
        Assert.AreEqual("Хлопець, який їй сподобався, — не хлопець", result.series);
        Assert.AreEqual("", result.authors);
        Assert.AreEqual(1, result.volumeNumber);
        Assert.AreEqual("https://nashaidea.com/wp-content/uploads/2025/05/guy01-sc-1080_result.webp", result.cover);
        Assert.AreEqual("Nasha Idea", result.publisher);
        Assert.AreEqual("Physical", result.type);
        Assert.AreEqual("978-617-8516-51-2", result.isbn);
        Assert.AreEqual(3, result.totalVolumes);
        Assert.AreEqual("ongoing", result.seriesStatus);
        Assert.AreEqual("https://nashaidea.com/product/ne-khlopets-tom-1/", result.url);
        Assert.AreEqual(DateTime.Parse("2025-06-06"), result.preorderStartDate);
        Assert.AreEqual(DateTime.Parse("2025-09-30"), result.release);
    }

    [TestMethod]
    public async Task NashaIdea_RegularVolume()
    {
        var parser = new PublisherParsersFactory().CreateParser("https://nashaidea.com/product/chi-tom-10/");

        Assert.IsNotNull(parser);

        var result = await parser.Parse();

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

    }

    [TestMethod]
    public async Task NashaIdea_OneShotVolume()
    {
        var parser = new PublisherParsersFactory().CreateParser("https://nashaidea.com/product/vitaiemo-v-koto-kafe/");

        Assert.IsNotNull(parser);

        var result = await parser.Parse();

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

    }


    [TestMethod]
    public async Task NashaIdea_FinishedSeries()
    {
        var parser = new PublisherParsersFactory().CreateParser("https://nashaidea.com/product/proshhavaj-troyandovyj-sade-tom-3/");

        Assert.IsNotNull(parser);

        var result = await parser.Parse();

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

    }


}
