using FluentAssertions;
using MangaShelf.BL.Contracts;
using MangaShelf.BL.Parsers;
using MangaShelf.DAL.Models;

namespace MangaShelf.Parser.Tests;

[TestClass]
public class MolfarParseTest : BaseParserTestClass<MolfarParser>
{
    static IEnumerable<object[]> TestInputs
    {
        get
        {
            return new[]
            {
                new object[]{
                    release.Url,
                    release
                },
                new object[]{
                    release2.Url,
                    release2
                },
                new object[]
                {
                    release3.Url,
                    release3
                }
            };
        }
    }

    [TestMethod]
    public async Task Molfar_PageExist_ShouldHaveVolumes()
    {
        var result = await Parser.GetVolumesUrls("https://molfarpublishing.ua/catalog/manga/page/2/", CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.HasCount(14, result);
    }

    [TestMethod]

    public async Task Molfar_PageExist_ShouldNotHaveVolumes()
    {
        await Assert.ThrowsExactlyAsync<HttpRequestException>(async () =>
        {
            await Parser.GetVolumesUrls("https://molfarpublishing.ua/catalog/manga/page/198/", CancellationToken.None);
        });
    }

    [TestMethod]
    [DynamicData(nameof(TestInputs))]
    public async Task ParseUrl(string url, ParsedInfo expectedValue)
    {
        var result = await Parser.Parse(url);

        result.Should().BeEquivalentTo(expectedValue, config => config
            .Excluding(x => x.Json)
            .Excluding(x=>x.Description)
            .Excluding(x => x.Cover));

        result.Description.Should().NotBeNullOrEmpty();
        result.Cover.Should().NotBeNullOrEmpty();
    }

    private static ParsedInfo release = new ParsedInfo()
    {
        CanBePublished = true,
        Title = "Том 4",
        CountryCode = "ua",
        Cover = "https://molfar-comics.com/wp-content/uploads/2024/03/bride04-jpg.webp",
        Isbn = "978-617-7885-93-0",
        IsPreorder = false,
        Publisher = "Molfar Comics",
        Release = null,
        Series = "Наречена чаклуна",
        SeriesStatus = SeriesStatus.Unknown,
        OriginalSeriesTitle = "яп. 魔法使いの嫁, Mahoutsukai no Yome",
        TotalVolumes =null,
        VolumeType = VolumeType.Physical,
        SeriesType = SeriesType.Unknown,
        AgeRestrictions = null,
        Url = "https://molfarpublishing.ua/product/narechena-chakluna-tom-4/",
        VolumeNumber = 4,
        Authors = "Ямадзакі Коре"
    };

    private static ParsedInfo release2 = new ParsedInfo()
    {
        CanBePublished = true,
        Title = "Том 1",
        CountryCode = "ua",
        Cover = "https://molfar-comics.com/wp-content/uploads/2025/04/cats01.jpg",
        Authors = "Hawkman,Mecha-Roots",
        Isbn = "978-617-8485-40-5",
        IsPreorder = false,
        Publisher = "Molfar Comics",
        Release = DateTime.SpecifyKind(new DateTime(2025, 12, 31), DateTimeKind.Local),
        Series = "Ніч Живих Нявців",
        SeriesStatus = SeriesStatus.Unknown,
        TotalVolumes = null,
        VolumeType = VolumeType.Physical,
        SeriesType = SeriesType.Unknown,
        AgeRestrictions = null,
        Url = "https://molfarpublishing.ua/product/nich-zhyvykh-niavtsiv-tom-1/",
        VolumeNumber = 1,
        OriginalSeriesTitle = "яп. ニャイト・オブ・ザ・リビングキャット, Nyaito obu za Ribingu Kyatto"
    };

    private static ParsedInfo release3 = new ParsedInfo()
    {
        CanBePublished = true,
        Title = "Том 3",
        CountryCode = "ua",
        Cover = "https://molfarpublishing.ua/static/d2b106e6a59eb3aa80dd4df9fa36b810/07686/cats03.webp",
        Authors = "Hawkman,Mecha-Roots",
        Isbn = "978-617-8485-67-2",
        IsPreorder = true,
        Publisher = "Molfar Comics",
        Release = DateTime.SpecifyKind(new DateTime(2026, 7, 31), DateTimeKind.Local),
        Series = "Ніч Живих Нявців",
        SeriesStatus = SeriesStatus.Unknown,
        TotalVolumes = null,
        VolumeType = VolumeType.Physical,
        SeriesType = SeriesType.Unknown,
        AgeRestrictions = null,
        Url = "https://molfarpublishing.ua/product/nich-zhyvykh-niavtsiv-tom-3/",
        VolumeNumber = 3,
        OriginalSeriesTitle = "яп. ニャイト・オブ・ザ・リビングキャット, Nyaito obu za Ribingu Kyatto"
    };
}