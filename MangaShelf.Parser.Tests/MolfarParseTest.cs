using FluentAssertions;
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
                }
            };
        }
    }

    [TestMethod]
    [DynamicData(nameof(TestInputs))]
    public async Task ParseUrl(string url, ParsedInfo expectedValue)
    {
        var result = await Parser.Parse(url);

        result.Should().BeEquivalentTo(expectedValue, config => config.Excluding(x => x.Json).Excluding(x=>x.Description));
        result.Description.Should().NotBeNullOrEmpty();
    }

    private static ParsedInfo release = new ParsedInfo()
    {
        CanBePublished = false,
        Title = "Том 4",
        CountryCode = "ua",
        Cover = "https://molfar-comics.com/wp-content/uploads/2024/03/bride04-jpg.webp",
        Isbn = string.Empty,
        IsPreorder = false,
        Publisher = "Molfar Comics",
        Release = null,
        Series = "Наречена чаклуна",
        SeriesStatus = SeriesStatus.Unknown,
        TotalVolumes = -1,
        VolumeType = VolumeType.Physical,
        SeriesType = SeriesType.Unknown,
        AgeRestrictions = null,
        Url = "https://molfar-comics.com/product/narechena-chakluna-tom-4/",
        VolumeNumber = 4
    };

    private static ParsedInfo release2 = new ParsedInfo()
    {
        CanBePublished = false,
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
        TotalVolumes = -1,
        VolumeType = VolumeType.Physical,
        SeriesType = SeriesType.Unknown,
        AgeRestrictions = null,
        Url = "https://molfar-comics.com/product/nich-zhyvykh-niavtsiv-tom-1/",
        VolumeNumber = 1
    };
}