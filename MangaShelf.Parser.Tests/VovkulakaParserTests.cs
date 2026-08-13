using MangaShelf.BL.Contracts;
using MangaShelf.BL.Parsers.Vovkulaka;
using MangaShelf.DAL.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace MangaShelf.Parser.Tests;

[TestClass]
public class VovkulakaParserTests : BaseParserTestClass<VovkulakaParser>
{
    [TestMethod]
    public void TestCanParse()
    {
        var result = this.Parser.CanParse("https://vovkulaka.net/vidmak-prokliattia-voroniv/");
        Assert.IsTrue(result, "Vovkulaka parser should be able to parse the given URL.");
    }

    [TestMethod]
    public async Task TestGetVolumesUrls()
    {
        for (int page = 1; page <= 3; page++)
        {
            var pageUrl = Parser.GetPageUrl(page);
            var result = await this.Parser.GetVolumesUrls(pageUrl, CancellationToken.None);
            Assert.HasCount(25, result);
        }
    }
    
    [TestMethod]
    [DataRow("https://vovkulaka.net/vidmak-prokliattia-voroniv/", "Прокляття Воронів")]
    [DataRow("https://vovkulaka.net/ostannia-dusha-tom-1/", "Том 1")]
    public async Task AssertTitle(string url, string expectedTitle)
    {
        var result = await Parser.Parse(url, CancellationToken.None);
        Assert.AreEqual(expectedTitle, result?.Title);
    }

    [TestMethod]
        [DataRow("https://vovkulaka.net/vidmak-prokliattia-voroniv/", "Відьмак")]
    [DataRow("https://vovkulaka.net/ostannia-dusha-tom-1/", "Остання душа")]
    public async Task AssertSeries(string url, string expectedTitle)
    {
        var result = await Parser.Parse(url, CancellationToken.None);
        Assert.AreEqual(expectedTitle, result?.Series);
    }

    [TestMethod]
    [DataRow("https://vovkulaka.net/vidmak-prokliattia-voroniv/", "Пол Тобін, Пйотр Ковальський")]
    public async Task AssertAuthors(string url, string expectedAuthors)
    {
        var result = await Parser.Parse(url, CancellationToken.None);
        Assert.AreEqual(expectedAuthors, result?.Authors);
    }

    [TestMethod]
    [DataRow("https://vovkulaka.net/vidmak-prokliattia-voroniv/", null)]
    [DataRow("https://vovkulaka.net/ostannia-dusha-tom-1/", 1)]
    public async Task AssertNumber(string url, int? expectedNumber)
    {
        var result = await Parser.Parse(url, CancellationToken.None);
        Assert.AreEqual(expectedNumber, result?.VolumeNumber);
    }

    [TestMethod]
    [DataRow("https://vovkulaka.net/vidmak-prokliattia-voroniv/", 16)]
    public async Task AssertAgeRestriction(string url, int? expectedAgeRestriction)
    {
        var result = await Parser.Parse(url, CancellationToken.None);
        Assert.AreEqual(expectedAgeRestriction, result.AgeRestrictions);
    }

    [TestMethod]
    [DataRow("https://vovkulaka.net/vidmak-prokliattia-voroniv/")]
    public async Task AssertCover(string url)
    {
        var result = await Parser.Parse(url, CancellationToken.None);
        Assert.IsNotNull(result.Cover);
        Assert.IsNotEmpty(result.Cover);
    }

    [TestMethod]
    [DataRow("https://vovkulaka.net/vidmak-prokliattia-voroniv/", "2023-12-31T00:00:00+02:00")]
    [DataRow("https://vovkulaka.net/vidmak-menshe-zlo/", "2026-12-31T00:00:00+02:00")]
    [DataRow("https://vovkulaka.net/svit-hry-cyberpunk-2077/", "2026-11-30T00:00:00+02:00")]
    public async Task AssertReleaseDate(string url, string expectedReleaseDate)
    {
        var result = await Parser.Parse(url, CancellationToken.None);

        var releaseDate = DateTimeOffset.Parse(expectedReleaseDate);
        Assert.AreEqual(releaseDate, result.Release);
    }

    [TestMethod]
    [DataRow("https://vovkulaka.net/vidmak-prokliattia-voroniv/", "9786177782079")]
    public async Task AssertISBN(string url, string expectedISBN)
    {
        var result = await Parser.Parse(url, CancellationToken.None);
        Assert.AreEqual(expectedISBN, result.Isbn);
    }

        [TestMethod]
    [DataRow("https://vovkulaka.net/vidmak-prokliattia-voroniv/")]
    public async Task AssertDescription(string url)
    {
        var result = await Parser.Parse(url, CancellationToken.None);
        Assert.IsNotNull(result.Description);
        Assert.IsNotEmpty(result.Description);
    }

    [TestMethod]
    [DataRow("https://vovkulaka.net/vidmak-prokliattia-voroniv/", false)]
    [DataRow("https://vovkulaka.net/vidmak-menshe-zlo/", true)]
    public async Task AssertIsPreorder(string url, bool expectedIsPreorder)
    {
        var result = await Parser.Parse(url, CancellationToken.None);
        Assert.AreEqual(expectedIsPreorder, result.IsPreorder);
    }  

    [TestMethod]
    [DataRow("https://vovkulaka.net/vidmak-prokliattia-voroniv/", SeriesType.Comic)]
    [DataRow("https://vovkulaka.net/ostannia-dusha-tom-1/", SeriesType.Manga)]
    [DataRow("https://vovkulaka.net/vdovychka-ta-rybokin-knyha-1/", SeriesType.GraphicNovel)]
    [DataRow("https://vovkulaka.net/svit-vidmaka/", SeriesType.Artbook)]
    public async Task AssertSeriesType(string url, SeriesType expectedSeriesType)
    {
        var result = await Parser.Parse(url, CancellationToken.None);
        Assert.AreEqual(expectedSeriesType, result.SeriesType);
    }
}