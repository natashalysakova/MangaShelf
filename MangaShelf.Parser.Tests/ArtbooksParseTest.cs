using FluentAssertions;
using MangaShelf.BL.Contracts;
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

        result.Should().BeEquivalentTo(expectedValue, config => config
            .Excluding(x => x.Json)
            .Excluding(x => x.Description)
            .Excluding(x => x.Cover));
        result.Description.Should().NotBeNullOrEmpty();
        result.Cover.Should().NotBeNullOrEmpty();
    }

    private static ParsedInfo preorder = new ParsedInfo()
    {
        CanBePublished = true,
        Series = "Дух в оболонці",
        Title = "Том 1",
        CountryCode = "ua",
        Cover = "https://artbooks.ua/image/cache/catalog/Covers/2026/Manga_ghost_in_the_shell_Vol.1_HQ-1551x1640.png",
        Authors = "Шіро Масамуне",
        Isbn = "9786175232477",
        IsPreorder = true,
        Release = null,
        PreorderStartDate = null,
        Publisher = "Artbooks",
        SeriesStatus = SeriesStatus.Unknown,
        TotalVolumes = null,
        VolumeType = VolumeType.Physical,
        SeriesType = SeriesType.Manga,
        AgeRestrictions = 16,
        Url = "https://artbooks.ua/dukh-v-obolontsi-tom-1",
        VolumeNumber = 1
    };

    private static ParsedInfo release2 = new ParsedInfo()
    {
        CanBePublished = true,
        Title = "Том 1",
        CountryCode = "ua",
        Cover = "https://artbooks.ua/image/cache/catalog/Covers/2025/%D0%BC%D0%B0%D0%BD%D2%91%D0%B0/Manga_Lie-In-April_Vol.1_HQ-1551x1640.png",
        Authors = "Наоші Аракава",
        Isbn = "9786175233245",
        IsPreorder = false,
        Release = null,
        PreorderStartDate = null,
        Publisher = "Artbooks",
        Series = "Твоя квітнева брехня",
        SeriesStatus = SeriesStatus.Unknown,
        TotalVolumes = null,
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
        Isbn = "9786175233214",
        IsPreorder = false,
        Publisher = "Artbooks",
        Release = null,
        Series = "Атака титанів",
        SeriesStatus = SeriesStatus.Unknown,
        TotalVolumes = null,
        VolumeType = VolumeType.Physical,
        SeriesType = SeriesType.Manga,
        AgeRestrictions = 14,
        Url = "https://artbooks.ua/ataka-titaniv-tom-2",
        VolumeNumber = 2
    };
}