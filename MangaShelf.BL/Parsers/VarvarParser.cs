using AngleSharp.Dom;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;

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

        var nodes = document.QuerySelectorAll(".product-meta-custom-double");
        foreach (var word in lookupWords)
        {
            var node = nodes.FirstOrDefault(x => x.TextContent.Escaped().StartsWith(word));
            if (node != null)
            {
                var author = node.TextContent.Escaped().Replace(word, "").Trim();
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
        var node = document.QuerySelector(".swiper-slide img");
        if (node is null)
            return string.Empty;

        return node.GetAttribute("src")!;
    }

    protected override string? GetISBN(IDocument document)
    {
        var nodes = document.QuerySelectorAll(".product-meta-custom-block");

        foreach (var node in nodes)
        {
            if (node.TextContent.Escaped().Contains("ISBN"))
            {
                var isbn = node.TextContent.Escaped().Replace("ISBN", "").Trim(' ', ':');

                if (isbn == "При Виборі Обкладинки")
                {
                    return IsbnFromVariations(document);
                }

                if (!string.IsNullOrEmpty(isbn))
                {
                    return isbn;
                }
            }
        }

        return null;
    }

    private string? IsbnFromVariations(IDocument document)
    {
        return null;
    }

    protected override bool GetIsPreorder(IDocument document)
    {
        var node = document.QuerySelector(".custom-preorder-badge");
        if (node != null && node.TextContent.Contains("ПЕРЕДЗАМОВЛЕННЯ", StringComparison.OrdinalIgnoreCase))
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
        var nodes = document.QuerySelectorAll(".product-meta-custom-single");

        var seasons = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "зима", 2 },
            { "весна", 5 },
            { "літо", 8 },
            { "осінь", 11 }
        };


        foreach (var node in nodes)
        {
            var text = node.TextContent.Escaped();
            if (text.Contains("Орієнтовна Дата Виходу:"))
            {
                var date = text.Replace("Орієнтовна Дата Виходу:", "").Trim(' ', ':');
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

                    var splittedDate = date.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    if (splittedDate.Length == 2)
                    {
                        var month = GetMonthNumber(splittedDate[0]);
                        int.TryParse(splittedDate[1], out var year);
                        if (month != null && year != default)
                        {
                            var day = DateTime.DaysInMonth(year, month.Value);
                            return DateTime.SpecifyKind(new DateTime(year, month.Value, day), DateTimeKind.Local);
                        }
                    }
                }
            }
        }

        return null;
    }

    static int? GetMonthNumber(string monthText)
    {
        var culture = new CultureInfo("uk-UA");
        var months = culture.DateTimeFormat.MonthNames
            .Concat(culture.DateTimeFormat.MonthGenitiveNames)
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Select((name, index) => new { name, index = (index % 12) + 1 })
            .ToDictionary(x => x.name, x => x.index, StringComparer.OrdinalIgnoreCase);

        return months.TryGetValue(monthText.Trim(), out var month) ? month : null;
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
        if (splitted.Length == 0)
            return title.Trim();

        return splitted[0].Trim();
    }

    protected override SeriesStatus GetSeriesStatus(IDocument document)
    {
        var title = GetVolumeTitle(document);
        var series = GetSeries(document);

        if (title == series)
        {
            return SeriesStatus.OneShot;
        }

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
        if (string.IsNullOrEmpty(volumeTitle))
        {
            return series;
        }
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

public static class StringExtentions
{
    public static string Escaped(this string input)
    {
        if (input is null)
        {
            return string.Empty;
        }

        return input
            .Replace("\r", string.Empty)
            .Replace("\n", string.Empty)
            .Replace("\t", string.Empty).Trim();
    }
}