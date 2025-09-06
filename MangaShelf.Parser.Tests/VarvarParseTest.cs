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
                    release.Url,
                    release
                },
                new object[]{
                    preorder.Url,
                    preorder
                },
                new object[]{
                    preorder2.Url,
                    preorder2
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
        result.Description.Should().NotBeNull();
    }

    private static ParsedInfo preorder = new ParsedInfo()
    {
        CanBePublished = false,
        Title = "том 2",
        CountryCode = "ua",
        Cover = "https://varvarpublishing.com/wp-content/uploads/2025/08/gangsta-02-cover_.webp",
        Isbn = "978-617-09-9780-7",
        IsPreorder = true,
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
    private static ParsedInfo release = new ParsedInfo()
    {
        CanBePublished = false,
        Title = "Том 1",
        CountryCode = "ua",
        Cover = "https://varvarpublishing.com/wp-content/uploads/2025/06/pig-1-cover-a.webp",
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

    private static ParsedInfo preorder2 = new ParsedInfo()
    {
        CanBePublished = false,
        VolumeNumber = 2,
        Title = "Том 2",
        CountryCode = "ua",
        Cover = "https://varvarpublishing.com/wp-content/uploads/2025/08/nebo-2-cov.webp",
        Authors = "Юн Інван,Кім Сонхі",
        Isbn = "978-617-09-9787-6",
        IsPreorder = true,
        Publisher = "Varvar Publishing",
        Release = DateTime.SpecifyKind(new DateTime(2025, 11,30), DateTimeKind.Local),
        PreorderStartDate = DateTime.SpecifyKind(new DateTime(2025, 8, 31), DateTimeKind.Local),
        Series = "Небо у безодні",
        SeriesStatus = SeriesStatus.Unknown,
        TotalVolumes = -1,
        VolumeType = VolumeType.Physical,
        SeriesType = SeriesType.Unknown,
        AgeRestrictions = null,
        Url = "https://varvarpublishing.com/product/nebo-u-bezodni-tom-2/"
    };
}