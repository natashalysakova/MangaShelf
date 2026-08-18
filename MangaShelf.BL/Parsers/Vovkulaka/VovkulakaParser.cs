using AngleSharp.Dom;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Parsers.Vovkulaka;


public class VovkulakaParser : BaseParser
{
    public VovkulakaParser(ILogger<VovkulakaParser> logger, IHtmlDownloader htmlDownloader) : base(logger, htmlDownloader)
    {
    }

    public override string SiteUrl => "https://vovkulaka.net/";

    public override string CatalogUrl => "katalog/";

    public override string Pagination => "page/{0}/";

    public override string VolumeTitleSelector => "#product-title";

    protected override int? GetAgeRestriction(IDocument document)
    {
        var ageRestrictionElement = GetFromTable(document, "Вік:");
        var splitted = ageRestrictionElement.Split(' ');
        if (splitted.Length == 3 && int.TryParse(splitted[1], out int age))
        {
            return age;
        }
        return null;
    }

    protected override string? GetAuthors(IDocument document)
    {
        var script = GetFromTable(document, "Сценарій:");
        var art = GetFromTable(document, "Ілюстрації:");

        if (!string.IsNullOrEmpty(script) && !string.IsNullOrEmpty(art))
        {
            return $"{script}, {art}";
        }
        else if (!string.IsNullOrEmpty(script))
        {
            return script;
        }
        else if (!string.IsNullOrEmpty(art))
        {
            return art;
        }

        return null;
    }

    private string GetFromTable(IDocument document, string label)
    {
        var tableRows = document.QuerySelectorAll(".product-specs__row");
        foreach (var row in tableRows)
        {
            var th = row.QuerySelector(".product-specs__term");
            if (th != null && th.TextContent.Trim() == label)
            {
                var td = row.QuerySelector(".product-specs__value");
                if (td != null)
                {
                    return td.TextContent.Trim();
                }
            }
        }
        return string.Empty;
    }

    protected override string GetCover(IDocument document)
    {
        var cover = document.QuerySelector(".product-gallery__image");
        if (cover == null)
        {
            _logger.LogWarning("Cover image element not found in the document.");
            return string.Empty;
        }

        return cover.GetAttribute("src") ?? string.Empty;
    }

    protected override string? GetDescription(IDocument document)
    {
        var node = document.QuerySelector(".product-desc-text");
        return node?.TextContent.Trim();
    }

    protected override string? GetISBN(IDocument document)
    {
        return GetFromTable(document, "ISBN:");
    }

    protected override bool GetIsPreorder(IDocument document)
    {
        var tags = document.QuerySelectorAll(".product-tags__item");
        foreach (var tag in tags)
        {
            if (tag.TextContent.Trim().ToLowerInvariant() == "передпродаж")
            {
                return true;
            }
        }
        return false;
    }

    protected override string? GetOriginalSeriesName(IDocument document)
    {
        return null;
    }

    protected override string GetPublisher(IDocument document)
    {
        return "Vovkulaka";
    }

    protected override DateTimeOffset? GetReleaseDate(IDocument document)
    {
        bool isPreorder = GetIsPreorder(document);
        if(isPreorder)
        {
            var lookupValue = "приблизний час відправки:";
            var text = document.Source.Text;
            var startIndex = text.IndexOf(lookupValue, StringComparison.OrdinalIgnoreCase);
            if(startIndex == -1)
            {
                _logger.LogWarning("Release date not found in the document.");
                return null;
            }
            var endIndex = text.IndexOf("</p>", startIndex);

            var releaseWindowString = text.Substring(startIndex + lookupValue.Length, endIndex - startIndex - lookupValue.Length);
            var split = releaseWindowString.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if(split.Length == 1)
            {
                return ParseYearIntoLastDayOfYear(split[0]);
            }
            if(split.Length == 2)
            {
                var month = split[0] switch
                {
                    "осінь" => 11,
                    "літо" => 8,
                    "весна" => 5,
                    "зима" => 2,
                    _ => 12
                };
                return ParseYearAndMonthIntoLastDayOfMonth(split[1], month);
            }
        }

        var releaseDateString = GetFromTable(document, "Рік видання:");
        return ParseYearIntoLastDayOfYear(releaseDateString);
    }

    protected override DateTimeOffset? GetSaleStartDate(IDocument document)
    {
        return null;
    }

    protected override string GetSeries(IDocument document)
    {
        var defaultNames = ParseHeader(document);

        if (defaultNames.Series != defaultNames.Title)
        {
            return defaultNames.Series;
        }

        var tags = document.QuerySelectorAll(".product-tags__item");
        foreach (var tag in tags)
        {
            if (tag.Attributes["href"].Value.Contains("/series/"))
            {
                var split = tag.TextContent.Split(" - ");
                if (split.Length > 1)
                {
                    return split[0].Trim();
                }

                return tag.TextContent.Trim();
            }
        }

        return defaultNames.Series;
    }

    protected override SeriesStatus GetSeriesStatus(IDocument document)
    {
        return SeriesStatus.Unknown;
    }

    protected override int? GetVolumeNumber(IDocument document)
    {
        var parsedHeader = ParseHeader(document);
        return parsedHeader.Number;
    }

    protected override string GetVolumeTitle(IDocument document)
    {
        var parsedHeader = ParseHeader(document);
        if (parsedHeader.Title != parsedHeader.Series)
        {
            return parsedHeader.Title;
        }

        var titleElement = document.QuerySelector(VolumeTitleSelector);

        if (titleElement == null)
        {
            _logger.LogWarning("Volume title element not found in the document.");
            return string.Empty;
        }

        var seriesTitle = GetSeries(document);

        var replaced = titleElement.TextContent.Replace(seriesTitle, string.Empty).Trim();
        if (replaced.StartsWith("."))
        {
            replaced = replaced.Substring(1).Trim();
        }

        return string.IsNullOrEmpty(replaced) ? parsedHeader.Title : replaced;
    }

    protected override VolumeType GetVolumeType(IDocument document)
    {
        return VolumeType.Physical;
    }

    protected override string GetVolumeUrlBlockClass()
    {
        return ".product-card__link";
    }

    protected override SeriesType GetSeriesType(IDocument document)
    {
        var tags = document.QuerySelectorAll(".product-tags__item");
        foreach (var tag in tags)
        {
            var tagValue = tag.TextContent.Trim().ToLowerInvariant();
            if (tagValue.Contains("манґа"))
            {
                return SeriesType.Manga;
            }
            else if (tagValue.Contains("комікси"))
            {
                return SeriesType.Comic;
            }
            else if (tagValue.Contains("артбук"))
            {
                return SeriesType.Artbook;
            }
            else if (tagValue.Contains("мальописи"))
            {
                return SeriesType.GraphicNovel;
            }
        }

        return SeriesType.Other;
    }
}
