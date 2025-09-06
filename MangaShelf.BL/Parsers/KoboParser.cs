namespace MangaShelf.BL.Parsers;

//public class KoboParser : BaseParser
//{
//    private readonly ILogger<KoboParser> _logger;

//    public KoboParser(ILogger<KoboParser> logger, 
//        [FromKeyedServices(HtmlDownloaderKeys.Advanced)] IHtmlDownloader htmlDownloader) : base(logger, htmlDownloader)
//    {
//        _logger = logger;
//    }

//    public override string SiteUrl => "https://www.kobo.com/";

//    public override string CatalogUrl => throw new NotImplementedException();

//    public override string Pagination => throw new NotImplementedException();

//    protected override string? GetAuthors(IDocument document)
//    {
//        var node = document.QuerySelector(".contributor-name");
//        return node?.TextContent;
//    }

//    protected override string GetCover(IDocument document)
//    {
//        var node = document.QuerySelector(".cover-image");
//        if (node == null)
//        {
//            return string.Empty;
//        }

//        var url = node.GetAttribute("src");
//        if (url == null)
//        {
//            return string.Empty;
//        }

//        return "https:" + url;
//    }

//    protected override DateTimeOffset? GetReleaseDate(IDocument document)
//    {
//        var nodes = document.QuerySelectorAll(".bookitem-secondary-metadata > ul > li");

//        foreach (var node in nodes)
//        {
//            if (node.TextContent.Contains("Release Date:"))
//            {
//                var date = node.Children[0].TextContent;
//                return DateTime.Parse(date);
//            }
//        }

//        return null;
//    }

//    protected override string GetSeries(IDocument document)
//    {
//        var node = document.QuerySelector(".product-sequence-field > a");
//        if (node == null)
//            return GetVolumeTitle(document);

//        return node.TextContent;
//    }

//    protected override string GetVolumeTitle(IDocument document)
//    {


//        var volume = GetVolumeNumber(document);

//        if (volume == -1)
//        {
//            var node = document.QuerySelector("h1.product-field");
//            if (node == null)
//                return string.Empty;

//            return node.TextContent.Trim([' ', '\n']);
//        }
//        else
//        {
//            return "Volume " + volume;
//        }

//    }

//    protected override int GetVolumeNumber(IDocument document)
//    {
//        var node = document.QuerySelector(".sequenced-name-prefix");
//        if (node == null)
//            return -1;
//        var text = node.TextContent;
//        var firstWhiteSpace = text.IndexOf(" ");
//        if (firstWhiteSpace == -1)
//            return -1;

//        var secondWhitespace = text.IndexOf(" ", firstWhiteSpace + 1);
//        var volume = text.Substring(firstWhiteSpace, secondWhitespace - firstWhiteSpace).Trim();

//        return int.Parse(volume);
//    }

//    protected override string GetPublisher(IDocument document)
//    {
//        var nodes = document.QuerySelectorAll(".bookitem-secondary-metadata > ul > li");

//        if (nodes == null || !nodes.Any())
//        {
//            return null;
//        }

//        return nodes.First().TextContent.Trim([' ', '\n']);
//    }

//    protected override VolumeType GetVolumeType(IDocument document)
//    {
//        return VolumeType.Digital;
//    }

//    protected override string GetISBN(IDocument document)
//    {
//        var nodes = document.QuerySelectorAll(".bookitem-secondary-metadata > ul > li");

//        foreach (var node in nodes)
//        {
//            if (node.TextContent.Contains("ISBN:"))
//            {
//                return node.Children[0].TextContent;
//            }
//        }

//        return string.Empty;
//    }

//    protected override SeriesStatus GetSeriesStatus(IDocument document)
//    {
//        return SeriesStatus.Unknown;
//    }

//    protected override string? GetOriginalSeriesName(IDocument document)
//    {
//        return null;
//    }

//    protected override DateTimeOffset? GetSaleStartDate(IDocument document)
//    {
//        throw new NotImplementedException();
//    }

//    protected override string GetCountryCode(IDocument document)
//    {
//        throw new NotImplementedException();
//    }

//    protected override bool GetIsPreorder(IDocument document)
//    {
//        throw new NotImplementedException();
//    }

//    protected override int? GetAgeRestriction(IDocument document)
//    {
//        throw new NotImplementedException();
//    }

//    protected override string GetVolumeUrlBlockClass()
//    {
//        throw new NotImplementedException();
//    }

//    protected override string? GetDescription(IDocument document)
//    {
//        throw new NotImplementedException();
//    }
//}