using AngleSharp.Dom;
using MangaShelf.BL.Enums;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Parsers.Malopus;
public class MalopusComicParser : MalopusParser
{
    public override string CatalogUrl => "/comics/";
    public MalopusComicParser(ILogger<MalopusComicParser> logger, [FromKeyedServices(HtmlDownloaderKeys.Malopus)] IHtmlDownloader htmlDownloader) : base(logger, htmlDownloader)
    {
    }

    protected override SeriesType GetSeriesType(IDocument document)
    {
        return SeriesType.Comic;
    }
}