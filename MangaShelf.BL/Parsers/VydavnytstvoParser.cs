using AngleSharp.Dom;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Parsers;

public class VydavnytstvoParser : BaseParser
{
    public VydavnytstvoParser(ILogger<BaseParser> logger, IHtmlDownloader htmlDownloader) : base(logger, htmlDownloader)
    {
    }

    public override string SiteUrl => "https://vydavnytstvo.com/";

    public override string CatalogUrl => "product-tag/manga";

    public override string Pagination => "/page/{0}/?per_page=36";

    protected override int? GetAgeRestriction(IDocument document)
    {
        var tags = document.QuerySelectorAll(".tagged_as a");
        foreach (var tag in tags)
        {
            if (tag.TextContent.EndsWith("+"))
            {
                var agePart = tag.TextContent.TrimEnd('+');
                if (int.TryParse(agePart, out int age))
                {
                    return age;
                }
            }
        }

        return null;
    }

    protected override string? GetAuthors(IDocument document)
    {
        var header = document.QuerySelector(".product_title");
        if (header == null)
        {
            return null;
        }

        var text = header.TextContent;

        var splitByBrackets = text.Split(',');
        return splitByBrackets[0].Trim();
    }

    protected override VolumeType GetVolumeType(IDocument document)
    {
        return VolumeType.Physical;
    }

    protected override string GetCover(IDocument document)
    {
        var node = document.QuerySelector(".woocommerce-product-gallery__image img");
        return node?.GetAttribute("src") ?? string.Empty;
    }

    protected override string GetISBN(IDocument document)
    {
        var node = document.QuerySelector(".woocommerce-product-attributes-item--attribute_isbn td");
        return node?.TextContent.Trim() ?? string.Empty;

    }

    protected override bool GetIsPreorder(IDocument document)
    {
        var header = document.QuerySelector(".product_title");
        if (header == null)
        {
            return false;
        }

        return header.TextContent.ToLower().Contains("(передзамовлення)");
    }

    protected override string? GetOriginalSeriesName(IDocument document)
    {
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

    protected override string GetPublisher(IDocument document)
    {
        return "Видавництво";
    }

    protected override DateTimeOffset? GetReleaseDate(IDocument document)
    {
        if (!GetIsPreorder(document))
        {
            return null;
        }

        var node = document.QuerySelector("#tab-description div p");
        if (node == null)
        {
            return null;
        }

        var text = node.TextContent;
        var lookupText = "Це передзамовлення! Чекаємо книжку з друку в";
        if (text.Contains(lookupText))
        {
            var monthPart = text.Substring(text.IndexOf(lookupText) + lookupText.Length).Trim('.', ' ');

            if (months.TryGetValue(monthPart, out int month))
            {
                var currentYear = DateTimeOffset.Now.Year;
                var currentMonth = DateTimeOffset.Now.Month;
                if (month < currentMonth)
                {
                    currentYear += 1; // next year
                }
                var day = DateTime.DaysInMonth(currentYear, month);
                return DateTime.SpecifyKind(new DateTime(currentYear, month, day), DateTimeKind.Local);
            }
        }
        return null;
    }

    private static Dictionary<string, int> months = new Dictionary<string, int>
    {
        { "січні", 1 },
        { "лютому", 2 },
        { "березні", 3 },
        { "квітні", 4 },
        { "травні", 5 },
        { "червні", 6 },
        { "липні", 7 },
        { "серпні", 8 },
        { "вересні", 9 },
        { "жовтні", 10 },
        { "листопаді", 11 },
        { "грудні", 12 }
    };

    protected override string GetSeries(IDocument document)
    {
        var header = document.QuerySelector(".product_title");
        if (header == null)
        {
            return string.Empty;
        }

        var indexOfComma = header.TextContent.IndexOf('«');
        var indexOfEndComma = header.TextContent.IndexOf('»');
        if (indexOfComma >= 0 && indexOfEndComma > indexOfComma)
        {
            return header.TextContent.Substring(indexOfComma + 1, indexOfEndComma - indexOfComma - 1).Trim();
        }

        return header.TextContent.Trim();
    }

    protected override SeriesStatus GetSeriesStatus(IDocument document)
    {
        return SeriesStatus.Unknown;
    }

    protected override string GetVolumeTitle(IDocument document)
    {
        var header = document.QuerySelector(".product_title");
        if (header == null)
        {
            return string.Empty;
        }

        var title = header.TextContent.Escaped();

        var indexOfEndComma = title.IndexOf('»');
        var indexOdLastDot = title.LastIndexOf('.');

        var indexToUse = indexOfEndComma > indexOdLastDot ? indexOfEndComma : indexOdLastDot;

        var indexOfPreorder = title.ToLower().IndexOf("(передзамовлення)");
        if (indexToUse >= 0)
        {
            if (indexOfPreorder > indexToUse)
            {
                return title.Substring(indexToUse + 1, indexOfPreorder - indexToUse - 1).Trim('.', ' ');
            }
            return title.Substring(indexToUse + 1).Trim();
        }

        return title.Trim();
    }


    protected override int GetVolumeNumber(IDocument document)
    {
        var header = document.QuerySelector(".product_title");
        if (header == null)
        {
            return -1;
        }

        if (header.TextContent.Contains("Том"))
        {
            var parts = header.TextContent.Split(' ');
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (parts[i].ToLower().Contains("том"))
                {
                    if (int.TryParse(parts[i + 1], out int number))
                    {
                        return number;
                    }
                }
            }
        }

        return -1;
    }

    protected override string GetVolumeUrlBlockClass()
    {
        return ".wd-entities-title a";
    }

    protected override bool GetCanBePublished()
    {
        return true;
    }
    protected override SeriesType GetSeriesType(IDocument document)
    {
        var tags = document.QuerySelectorAll(".tagged_as a").Select(x => x.TextContent);

        if (tags.Contains("манґа") || tags.Contains("манга"))
        {
            return SeriesType.Manga;
        }
        if (tags.Contains("комікс") || tags.Contains("comic"))
        {
            return SeriesType.Comic;
        }

        return SeriesType.Unknown;
    }

    protected override string? GetDescription(IDocument document)
    {
        var node = document.QuerySelector("#tab-description");
        return node?.TextContent;
    }
}