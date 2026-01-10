using FluentAssertions;
using MangaShelf.BL.Parsers;
using MangaShelf.DAL.Models;

namespace MangaShelf.Parser.Tests;

[TestClass]
public class ArtbooksParseTest : BaseParserTestClass<ArtbooksParser>
{
    static IEnumerable<object[]> TestInputs
    {
        get
        {
            return new[]
            {
                new object[]{
                    preorder.Url,
                    preorder
                },
                new object[]{
                    release.Url,
                    release
                }
            };
        }
    }

    [TestMethod]
    [DynamicData(nameof(TestInputs))]
    public async Task ParseUrl(string url, ParsedInfo expectedValue)
    {
        var result = await Parser.Parse(url);

        result.Should().BeEquivalentTo(expectedValue, config => config
            .Excluding(x => x.Json)
            .Excluding(x => x.Description));
        result.Description.Should().NotBeNullOrEmpty();
    }

    private static ParsedInfo preorder = new ParsedInfo()
    {
        CanBePublished = true,
        Title = "Том 1",
        CountryCode = "ua",
        Cover = "https://artbooks.ua/image/cache/catalog/Covers/2025/%D0%BC%D0%B0%D0%BD%D2%91%D0%B0/Manga_Lie-In-April_Vol.1_HQ-1551x1640.png",
        Authors = "Наоші Аракава",
        Isbn = "978-617-523-324-5",
        IsPreorder = true,
        Release = DateTime.SpecifyKind(new DateTime(2025, 10, 1), DateTimeKind.Local),
        PreorderStartDate = null,
        Publisher = "Artbooks",
        Series = "Твоя квітнева брехня",
        SeriesStatus = SeriesStatus.Unknown,
        TotalVolumes = -1,
        VolumeType = VolumeType.Physical,
        SeriesType = SeriesType.Manga,
        AgeRestrictions = 14,
        Url = "https://artbooks.ua/tvoia-kvitneva-brekhnia-tom-1",
        VolumeNumber = 1
    };

    private static ParsedInfo release = new ParsedInfo()
    {
        CanBePublished = true,
        Title = "Том 2",
        CountryCode = "ua",
        Cover = "https://artbooks.ua/image/cache/catalog/Covers/2025/%D0%BC%D0%B0%D0%BD%D2%91%D0%B0/Manga_ATTACK_ON_TITAN_Vol.2_web%20%281%29-700x740.png",
        Authors = "Хаджіме Ісаяма",
        Isbn = "978-617-523-321-4",
        IsPreorder = false,
        Publisher = "Artbooks",
        Release = DateTime.SpecifyKind(new DateTime(2025, 12, 31), DateTimeKind.Local),
        Series = "Атака титанів",
        SeriesStatus = SeriesStatus.Unknown,
        TotalVolumes = -1,
        VolumeType = VolumeType.Physical,
        SeriesType = SeriesType.Manga,
        AgeRestrictions = 14,
        Url = "https://artbooks.ua/ataka-titaniv-tom-2",
        VolumeNumber = 2
    };
}