using FluentAssertions;
using MangaShelf.BL.Parsers;
using MangaShelf.DAL.Models;

namespace MangaShelf.Parser.Tests;

[TestClass]
public class VarvarParseTest : BaseParserTestClass<VarvarParser>
{
    static IEnumerable<object[]> TestInputs
    {
        get
        {
            return new[]
            {
                new object[]{
                    released.Url,
                    released
                },
                new object[]{
                    preorder.Url,
                    preorder
                },
                new object[]{
                    released2.Url,
                    released2
                }
            };
        }
    }

    [TestMethod]
    [DynamicData(nameof(TestInputs))]
    public async Task ParseUrl(string url, ParsedInfo expectedValue)
    {
        var result = await Parser.Parse(url);

        result.Should()
            .BeEquivalentTo(expectedValue, config => config
                .Excluding(x => x.Json)
                .Excluding(x=>x.Description));

        result.Description.Should().NotBeNull();
    }

    private static ParsedInfo released = new ParsedInfo()
    {
        CanBePublished = false,
        Title = "Том 2",
        CountryCode = "ua",
        Cover = "https://varvarpublishing.com/wp-content/uploads/2025/08/554231_cover-200x200.webp",
        Isbn = "978-617-09-9780-7",
        IsPreorder = false,
        Authors = "Косуке",
        Publisher = "Varvar Publishing",
        Release = null,
        Series = "Ґенґста",
        SeriesStatus = SeriesStatus.Unknown,
        TotalVolumes = -1,
        VolumeType = VolumeType.Physical,
        SeriesType = SeriesType.Unknown,
        AgeRestrictions = null,
        Url = "https://varvarpublishing.com/product/gengsta-tom-2/",
        PreorderStartDate = DateTime.SpecifyKind(new DateTime(2025, 8, 31), DateTimeKind.Local),
        VolumeNumber = 2
    };
    private static ParsedInfo released2 = new ParsedInfo()
    {
        CanBePublished = false,
        Title = "Том 1",
        CountryCode = "ua",
        Cover = "https://varvarpublishing.com/wp-content/uploads/2025/06/549426_cover-200x200.webp",
        Authors = "Кім Карнбі,Чхон Бомшік",
        Isbn = null,
        IsPreorder = false,
        Publisher = "Varvar Publishing",
        Release = null,
        Series = "Свинарня",
        SeriesStatus = SeriesStatus.Unknown,
        TotalVolumes = -1,
        VolumeType = VolumeType.Physical,
        SeriesType = SeriesType.Unknown,
        AgeRestrictions = null,
        Url = "https://varvarpublishing.com/product/svynarnya-tom-1/",
        VolumeNumber = 1,
        PreorderStartDate = DateTime.SpecifyKind(new DateTime(2025, 6, 30), DateTimeKind.Local),
    };

    private static ParsedInfo preorder = new ParsedInfo()
    {
        CanBePublished = false,
        VolumeNumber = -1,
        Title = "Бібліоманія",
        CountryCode = "ua",
        Cover = "https://varvarpublishing.com/wp-content/uploads/2025/12/556003_cover-200x200.webp",
        Authors = "Орвал,Маччіро",
        Isbn = "978-617-09-9919-1",
        IsPreorder = true,
        Publisher = "Varvar Publishing",
        Release = DateTime.SpecifyKind(new DateTime(2026, 02,28), DateTimeKind.Local),
        PreorderStartDate = DateTime.SpecifyKind(new DateTime(2025, 12, 31), DateTimeKind.Local),
        Series = "Бібліоманія",
        SeriesStatus = SeriesStatus.OneShot,
        TotalVolumes = -1,
        VolumeType = VolumeType.Physical,
        SeriesType = SeriesType.Unknown,
        AgeRestrictions = null,
        Url = "https://varvarpublishing.com/bibliomaniya/"
    };
}