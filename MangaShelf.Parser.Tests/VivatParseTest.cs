using AngleSharp.Dom;
using FluentAssertions;
using MangaShelf.BL.Contracts;
using MangaShelf.BL.Parsers;
using MangaShelf.DAL.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace MangaShelf.Parser.Tests
{
    [TestClass]
    public class VivatParseTest : BaseParserTestClass<VivatParser>
    {
        [TestMethod]
        public async Task ParseGetVolumesUrls_ShouldReturnVolumeUrls()
        {
            var result = await Parser.GetVolumesUrls("https://vivat.com.ua/category/manga/?page=1", CancellationToken.None);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count() > 0);
        }

        [TestMethod]
        [DataRow("https://vivat.com.ua/product/vykhidnyi-den-lykhodiia-1/", true)]
        [DataRow("https://vivat.com.ua/product/tsia-portselianova-lialechka-zakokhalasia-tom-1/", true)]
        [DataRow("https://malopus.com.ua/manga/manga-cya-porcelyanova-lyalechka-zakohalasya-tom-1/", false)]
        public async Task ParseCanParse_ShouldReturnCorrectValue(string url, bool expected)
        {
            var result = Parser.CanParse(url);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public async Task ParseGetPageUrl_ShouldReturnFormattedUrl()
        {
            var result = Parser.GetPageUrl(5);
            Assert.AreEqual("https://vivat.com.ua/category/manga/?page=5", result);
        }


        [TestMethod]
        [DynamicData(nameof(TestInputs))]
        public async Task ParseVolume_ShouldReturnParsedVolume(string url, ParsedInfo expectedResult)
        {
            var result = await Parser.Parse(url, CancellationToken.None);

            Assert.IsNotNull(result);

            result
                .Should()
                .BeEquivalentTo(expectedResult, options => options
                .Excluding(x => x.Json)
                .Excluding(x => x.Description)
                .Excluding(x => x.Cover));

            Assert.Contains(Parser.SiteUrl, result.Cover);
        }

        static IEnumerable<object[]> TestInputs
        {
            get
            {
                return new[]
                {
                new object[]{
                    panLykhodiy.Url,
                    panLykhodiy
                },
                new object[]{
                    grandBlue.Url,
                    grandBlue
                }};
            }
        }

        static ParsedInfo panLykhodiy = new()
        {
            AgeRestrictions = null,
            Authors = "Ю Морікава",
            CanBePublished = true,
            CountryCode = "ua",
            Cover = "https://vivat.com.ua/resize_720x1029x95/storage/1.d/files/e/6/e6de035a_vykhidnyi-den-lykhodiia-tom-1.webp",
            Isbn = "9786171713260",
            IsPreorder = false,
            OriginalSeriesTitle = "Kyūjitsu no Warumono-san #1 by Yuu Morikawa",
            Publisher = "Vivat",
            Release = DateTimeOffset.Parse("2025-12-31T00:00:00 +02:00"),
            Series = "Вихідний день Лиходія",
            PreorderStartDate = null,
            SeriesStatus = SeriesStatus.Unknown,
            SeriesType = SeriesType.Manga,
            Title = "Том 1",
            TotalVolumes = null,
            Url = "https://vivat.com.ua/product/vykhidnyi-den-lykhodiia-1/",
            VolumeNumber = 1,
            VolumeType = VolumeType.Physical
        };

        static ParsedInfo grandBlue = new()
        {
            AgeRestrictions = null,
            Authors = "Кенджі Іноуе,Кімітаке Йошіока",
            CanBePublished = true,
            CountryCode = "ua",
            Cover = "https://vivat.com.ua/resize_720x1029x95/storage/1.d/files/e/6/e6de035a_vykhidnyi-den-lykhodiia-tom-1.webp",
            Isbn = "9786178168674",
            IsPreorder = false,
            OriginalSeriesTitle = "Grand Blue. Vol. 1-2 by Kenji Inoue, Kimitake Yoshioka (Illustrator)",
            Publisher = "MAL'OPUS",
            Release = DateTimeOffset.Parse("2025-12-31T00:00:00 +02:00"),
            Series = "Неосяжна блакить",
            PreorderStartDate = null,
            SeriesStatus = SeriesStatus.Unknown,
            SeriesType = SeriesType.Manga,
            Title = "Омнібус 1 (Томи 1–2)",
            TotalVolumes = null,
            Url = "https://vivat.com.ua/product/neosiazhna-blakyt-1",
            VolumeNumber = 1,
            VolumeType = VolumeType.Physical
        };
    }
}
