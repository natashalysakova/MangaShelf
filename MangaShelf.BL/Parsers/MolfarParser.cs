using AngleSharp.Dom;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Parsers;

public class MolfarParser : BaseParser
{
    public MolfarParser(ILogger<BaseParser> logger,  IHtmlDownloader htmlDownloader) : base(logger, htmlDownloader)
    {
    }

    public override string SiteUrl => "https://molfar-comics.com";

    public override string CatalogUrl => "/product-category/manga";

    public override string Pagination => "/page/{0}/";

    protected override int? GetAgeRestriction(IDocument document)
    {
        return null;
    }

    private static string? GetFromTable(IDocument document, string fieldName)
    {
        var rows = document.QuerySelectorAll(".WrapAttrLast");
        foreach (var row in rows)
        {
            var header = row.QuerySelector("strong");
            if (header != null && header.TextContent.Trim().Equals(fieldName, StringComparison.OrdinalIgnoreCase))
            {
                var value = row.QuerySelector("span");
                if (value != null)
                {
                    return value.TextContent.Trim();
                }
            }
        }
        return null;
    }

    protected override string? GetAuthors(IDocument document)
    {
        var authors = GetFromTable(document, "Автор");
        if (authors != null)
        {
            var splitted = authors.Split(new[] { ';', '&', '/' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            authors = string.Join(",", splitted);
        }

        return authors;
    }

    protected override VolumeType GetVolumeType(IDocument document)
    {
        var ePurchase = document.QuerySelector(".e-purchase");
        if (ePurchase != null && ePurchase.TextContent.Trim() != string.Empty)
        {
            return VolumeType.Digital | VolumeType.Physical;
        }

        return VolumeType.Physical;
    }

    protected override string GetCover(IDocument document)
    {
        var coverNode = document.QuerySelector(".woocommerce-product-gallery__wrapper .swiper-slide img");
        if (coverNode is null)
            return string.Empty;

        return coverNode.GetAttribute("src")!;
    }

    protected override string GetISBN(IDocument document)
    {
        return GetFromTable(document, "ISBN") ?? string.Empty;
    }

    protected override bool GetIsPreorder(IDocument document)
    {
        return false;
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
        return "Molfar Comics";
    }

    protected override DateTimeOffset? GetReleaseDate(IDocument document)
    {
        var node = GetFromTable(document, "Рік видання");
        return ParseYearIntoLastDayOfYear(node);
    }

    protected override string GetSeries(IDocument document)
    {
        var titleNode = document.QuerySelector(".product_title");
        if (titleNode is null)
            return string.Empty;

        return GetSeriesNameFromDefaultTitle(titleNode.TextContent);
    }

    protected override SeriesStatus GetSeriesStatus(IDocument document)
    {
        return SeriesStatus.Unknown;
    }

    protected override string GetVolumeTitle(IDocument document)
    {
        var titleNode = document.QuerySelector(".product_title");
        if (titleNode is null)
            return string.Empty;

        return GetVolumeTitleFromDefaultTitle(titleNode.TextContent);

    }

    protected override int GetVolumeNumber(IDocument document)
    {
        var titleNode = document.QuerySelector(".product_title");
        if (titleNode is null)
            return -1;

        return GetVolumeNumberFromDefaultTitle(titleNode.TextContent);

    }

    protected override string GetVolumeUrlBlockClass()
    {
        return ".wrap-title-product";
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
        var descNode = document.QuerySelector(".desc");
        return descNode?.TextContent;
    }
}