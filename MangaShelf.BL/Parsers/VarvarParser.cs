using AngleSharp.Dom;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Parsers;

public class VarvarParser : BaseParser
{
    public VarvarParser(ILogger<BaseParser> logger, IHtmlDownloader htmlDownloader) : base(logger, htmlDownloader)
    {
    }

    public override string SiteUrl => "https://varvarpublishing.com/";

    public override string CatalogUrl => "shop/";

    public override string Pagination => "?product-page={0}";

    protected override int? GetAgeRestriction(IDocument document)
    {
        return null;
    }

    protected override string? GetAuthors(IDocument document)
    {
        List<string> authors = new List<string>();
        var lookupWords = new[] { "Автор:", "Сценарист:", "Художник:", "Автор, Художник:" };

        var nodes = document.QuerySelectorAll(".woocommerce-product-details__short-description p");
        foreach (var word in lookupWords)
        {
            var node = nodes.FirstOrDefault(x => x.TextContent.StartsWith(word));
            if (node != null)
            {
                var author = node.TextContent.Replace(word, "").Trim();
                if (!string.IsNullOrEmpty(author) && !authors.Contains(author))
                {
                    authors.Add(author);
                }
            }
        }

        if (authors.Any())
        {
            return string.Join(',', authors);
        }

        return null;
    }

    protected override VolumeType GetVolumeType(IDocument document)
    {
        return VolumeType.Physical;
    }

    protected override string GetCover(IDocument document)
    {
        var node = document.QuerySelector(".woocommerce-product-gallery__image a");
        if (node is null)
            return string.Empty;

        return node.GetAttribute("href")!;
    }

    protected override string? GetISBN(IDocument document)
    {
        var nodes = document.QuerySelectorAll(".woocommerce-product-details__short-description p");

        foreach (var node in nodes)
        {
            if (node.TextContent.Contains("ISBN"))
            {
                var isbn = node.TextContent.Replace("ISBN", "").Trim(' ', ':');
                if (!string.IsNullOrEmpty(isbn))
                {
                    return isbn;
                }
            }
        }

        return null;
    }

    protected override bool GetIsPreorder(IDocument document)
    {
        var node = document.QuerySelector(".onsale");
        if (node != null && node.TextContent.Contains("Передзамовлення", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    protected override string? GetOriginalSeriesName(IDocument document)
    {
        return null;
    }

    protected override string GetPublisher(IDocument document)
    {
        return "Varvar Publishing";
    }

    protected override DateTimeOffset? GetReleaseDate(IDocument document)
    {
        var nodes = document.QuerySelectorAll(".woocommerce-product-details__short-description p");

        var seasons = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "зима", 2 },
            { "весна", 5 },
            { "літо", 8 },
            { "осінь", 11 }
        };

        foreach (var node in nodes)
        {
            if (node.TextContent.Contains("Орієнтовна дата виходу:"))
            {
                var date = node.TextContent.Replace("Орієнтовна дата виходу:", "").Trim(' ', ':');
                if (!string.IsNullOrEmpty(date))
                {
                    foreach (var season in seasons)
                    {
                        if (date.Contains(season.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            var yearPart = date.Replace(season.Key, "").Trim(' ', ',');
                            if (int.TryParse(yearPart, out int year))
                            {
                                var day = DateTime.DaysInMonth(year, season.Value);
                                return DateTime.SpecifyKind(new DateTime(year, season.Value, day), DateTimeKind.Local);
                            }
                        }
                    }
                }
            }
        }

        return null;
    }

    protected override DateTimeOffset? GetSaleStartDate(IDocument document)
    {
        var coverUrl = GetCover(document);
        if (string.IsNullOrEmpty(coverUrl))
        {
            return null;
        }
        var word = "/uploads/";
        var indexOfDate = coverUrl.IndexOf(word);
        var datePart = coverUrl.Substring(indexOfDate + word.Length, 7); // "2025/08"
        if (DateTime.TryParseExact(datePart, "yyyy/MM", null, System.Globalization.DateTimeStyles.None, out DateTime date))
        {
            var day = DateTime.DaysInMonth(date.Year, date.Month);
            return DateTime.SpecifyKind(new DateTime(date.Year, date.Month, day), DateTimeKind.Local);
        }

        return null;
    }

    protected override string GetSeries(IDocument document)
    {
        var node = document.QuerySelector(".entry-title");
        if (node is null)
            return string.Empty;

        var title = node.TextContent;

        var splitted = title.Split(new[] { "том", "Том" }, StringSplitOptions.RemoveEmptyEntries);
        if(splitted.Length == 0)
            return title.Trim();

        return splitted[0].Trim();
    }

    protected override SeriesStatus GetSeriesStatus(IDocument document)
    {
        return SeriesStatus.Unknown;
    }

    protected override string GetVolumeTitle(IDocument document)
    {
        var series = GetSeries(document);
        var node = document.QuerySelector(".entry-title");
        if (node is null)
            return string.Empty;
        var title = node.TextContent;
        var volumeTitle = title.Replace(series, "").Trim();
        return volumeTitle;
    }

    protected override int GetVolumeNumber(IDocument document)
    {
        var node = document.QuerySelector(".entry-title");
        if (node is null)
            return -1;
        var title = node.TextContent;
        var indexOfVolume = title.IndexOf("том ", StringComparison.OrdinalIgnoreCase);
        if (indexOfVolume == -1)
            return -1;

        var volumePart = title.Substring(indexOfVolume + 4).Trim();
        return int.TryParse(volumePart, out int volumeNumber) ? volumeNumber : -1;
    }

    protected override string GetVolumeUrlBlockClass()
    {
        return ".woocommerce-loop-product__link";
    }

    protected override bool GetCanBePublished()
    {
        return false;
    }
    protected override SeriesType GetSeriesType(IDocument document)
    {
        return SeriesType.Unknown;
    }

    protected override string? GetDescription(IDocument document)
    {
        var node = document.QuerySelector(".woocommerce-Tabs-panel--description p");
        if (node is null)
            return null;

        return node.TextContent.Trim();
    }
}