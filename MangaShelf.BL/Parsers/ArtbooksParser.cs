using AngleSharp.Dom;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Parsers;

public class ArtbooksParser : BaseParser
{
    public ArtbooksParser(ILogger<BaseParser> logger,  IHtmlDownloader htmlDownloader) : base(logger, htmlDownloader)
    {
    }

    public override string SiteUrl => "https://artbooks.ua";

    public override string CatalogUrl => "/manga";

    public override string Pagination => "/?page={0}";

    private string? GetFromTable(IDocument document, string column)
    {
        var nodes = document.QuerySelectorAll(".info-book__right tr");
        foreach (var node in nodes)
        {
            var tds = node.QuerySelectorAll("td");
            if (tds.Count() != 2)
                continue;

            if (tds[0].TextContent.Trim().ToLower().Contains(column.ToLower()))
            {
                return tds[1].TextContent.Trim();
            }
        }
        return null;
    }

    protected override int? GetAgeRestriction(IDocument document)
    {
        var categories = GetFromTable(document, "Категорії:");
        if (categories is null || string.IsNullOrWhiteSpace(categories))
            return null;

        var splitted = categories.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (splitted is null || splitted.Length == 0)
            return null;

        var ageString = splitted.FirstOrDefault(x => x.EndsWith('+'));
        if (ageString is null)
            return null;

        ageString = ageString.TrimEnd('+');
        if (int.TryParse(ageString, out var age))
            return age;

        return null;
    }

    protected override string? GetAuthors(IDocument document)
    {
        return GetFromTable(document, "Автор/ка:");
    }

    protected override VolumeType GetVolumeType(IDocument document)
    {
        return VolumeType.Physical;
    }

    protected override string GetCover(IDocument document)
    {
        var coverNode = document.QuerySelector(".main-book__top");
        if (coverNode is null)
            return string.Empty;

        return coverNode.GetAttribute("src")!;
    }

    protected override string GetISBN(IDocument document)
    {
        return GetFromTable(document, "ISBN:") ?? string.Empty;
    }

    protected override bool GetIsPreorder(IDocument document)
    {
        var stockNode = document.QuerySelector(".stock-presale");
        return stockNode is not null;
    }

    protected override string? GetOriginalSeriesName(IDocument document)
    {
        return null;
    }

    protected override DateTimeOffset? GetSaleStartDate(IDocument document)
    {
        return null;
    }

    protected override string GetPublisher(IDocument document)
    {
        return "Artbooks";
    }

    protected override DateTimeOffset? GetReleaseDate(IDocument document)
    {
        // If it is preorder, then try to get exact date
        var node = document.QuerySelector("div .stock-wait");
        if (node != null)
        {
            var dateNode = node.QuerySelector("._d_bold");
            if (dateNode is null)
                return null;

            if (DateTimeOffset.TryParseExact(dateNode.TextContent, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.AssumeLocal, out DateTimeOffset parsedExactDate))
            {
                return parsedExactDate;
            }
        }

        // Not a preorder, or exact date not found, try to get year only
        //var year = GetFromTable(document, "Рік:");
        //if (year is null || string.IsNullOrWhiteSpace(year))
        //    return null;

        //return ParseYearIntoLastDayOfYear(year);

        return null;
    }

    protected override string GetSeries(IDocument document)
    {
        var node = document.QuerySelector(".title");
        var title = node.TextContent;

        return GetSeriesNameFromDefaultTitle(title);
    }

    protected override SeriesStatus GetSeriesStatus(IDocument document)
    {
        return SeriesStatus.Unknown;
    }

    protected override string GetVolumeTitle(IDocument document)
    {
        var node = document.QuerySelector(".title");
        var title = node.TextContent;
        return GetVolumeTitleFromDefaultTitle(title);
    }

    protected override int GetVolumeNumber(IDocument document)
    {
        var node = document.QuerySelector(".title");
        var title = node.TextContent;
        return GetVolumeNumberFromDefaultTitle(title);
    }

    protected override string GetVolumeUrlBlockClass()
    {
        return ".product-thumb";
    }

    protected override string? GetDescription(IDocument document)
    {
        var descNode = document.QuerySelector(".description__text");
        var text = descNode?.TextContent.Trim();
        return text;
    }
}
