using AngleSharp.Dom;
using MangaShelf.BL.Enums;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Parsers.Malopus;

public class MalopusCookbooksParser : MalopusParser
{
    public override string CatalogUrl => "/cookbooks/";
    public MalopusCookbooksParser(ILogger<MalopusCookbooksParser> logger, [FromKeyedServices(HtmlDownloaderKeys.Malopus)] IHtmlDownloader htmlDownloader) : base(logger, htmlDownloader)
    {
    }

    protected override SeriesType GetSeriesType(IDocument document)
    {
        return SeriesType.Cookbook;
    }

    protected override string GetSeries(IDocument document)
    {
        var node = document.QuerySelector(".product-title");
        if (node == null)
        {
            throw new Exception("Series title node not found");
        }

        var name = node.TextContent.ToString().Trim();
        if (name.StartsWith("Кулінарна книга"))
        {
            name = name.Replace("Кулінарна книга", string.Empty).Trim();
        }

        return name;
    }

    protected override string GetVolumeTitle(IDocument document)
    {
        return GetSeries(document);
    }
}
