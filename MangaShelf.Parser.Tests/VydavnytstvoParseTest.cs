using FluentAssertions;
using MangaShelf.BL.Parsers;
using MangaShelf.DAL.Models;

namespace MangaShelf.Parser.Tests;

[TestClass]
public class VydavnytstvoParseTest : BaseParserTestClass<VydavnytstvoParser>
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

        result
            .Should().BeEquivalentTo(expectedValue, config => config
            .Excluding(x => x.Json)
            .Excluding(x=>x.Description));
        result.Description.Should().NotBeNullOrEmpty();

    }

    private static ParsedInfo release2 = new ParsedInfo()
    {
        CanBePublished = true,
        Title = "Том 1",
        CountryCode = "ua",
        Cover = "https://i0.wp.com/vydavnytstvo.com/wp-content/uploads/2025/03/kobayashi_v1.jpg?fit=600%2C600&ssl=1",
        Isbn = "978-617-8149-23-9",
        IsPreorder = false,
        Publisher = "Видавництво",
        Authors = "Кулкьошінджя",
        Release = null,
        Series = "Драконопокоївка в домі Кобаяші-сан",
        PreorderStartDate = DateTime.SpecifyKind(new DateTime(2025, 03, 31), DateTimeKind.Local),
        SeriesStatus = SeriesStatus.Unknown,
        TotalVolumes = -1,
        VolumeType = VolumeType.Physical,
        SeriesType = SeriesType.Manga,
        AgeRestrictions = 16,
        Url = "https://vydavnytstvo.com/shop/kobayashi-v1/",
        VolumeNumber = 1
    };
    private static ParsedInfo release = new ParsedInfo()
    {
        CanBePublished = true,
        Title = "Том 1",
        CountryCode = "ua",
        Cover = "https://i0.wp.com/vydavnytstvo.com/wp-content/uploads/2024/07/mydiary1.jpg?fit=600%2C600&ssl=1",
        Isbn = "978-617-8149-09-3",
        IsPreorder = false,
        Publisher = "Видавництво",
        Authors = "Наґата Кабі",
        Series = "Щоденник стосунків самої із собою",
        SeriesStatus = SeriesStatus.Unknown,
        PreorderStartDate = DateTime.SpecifyKind(new DateTime(2024, 07, 31), DateTimeKind.Local),
        TotalVolumes = -1,
        VolumeType = VolumeType.Physical,
        SeriesType = SeriesType.Manga,
        AgeRestrictions = 16,
        Url = "https://vydavnytstvo.com/shop/mydiary1/",
        VolumeNumber = 1
    };

    public VydavnytstvoParseTest()
    {
    }
}