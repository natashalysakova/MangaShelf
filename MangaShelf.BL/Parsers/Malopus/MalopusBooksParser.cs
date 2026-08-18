using AngleSharp.Dom;
using MangaShelf.BL.Enums;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Parsers.Malopus;

public class MalopusBooksParser : MalopusParser
{
    public override string CatalogUrl => "/books/";
    public MalopusBooksParser(ILogger<MalopusBooksParser> logger, [FromKeyedServices(HtmlDownloaderKeys.Malopus)] IHtmlDownloader htmlDownloader) : base(logger, htmlDownloader)
    {
    }

    protected override SeriesType GetSeriesType(IDocument document)
    {
        return SeriesType.Book;
    }

    protected override SeriesStatus GetSeriesStatus(IDocument document)
    {
        return SeriesStatus.OneShot;
    }

    protected override string GetSeries(IDocument document)
    {
        var node = document.QuerySelector(".product-title");
        if (node == null)
        {
            throw new Exception("Series title node not found");
        }

        var name = node.TextContent.ToString().Trim();
        if (name.StartsWith("Книга-артбук"))
        {
            name = name.Replace("Книга-артбук", string.Empty).Trim();
        }

        if (name.StartsWith("Книга"))
        {
            name = name.Replace("Книга", string.Empty).Trim();
        }

        return name;
    }

    protected override string GetVolumeTitle(IDocument document)
    {
        return GetSeries(document);
    }
}