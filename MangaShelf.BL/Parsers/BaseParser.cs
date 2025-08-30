using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using MangaShelf.Common;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

namespace MangaShelf.BL.Parsers;

public abstract class BaseParser : IPublisherParser
{
    private readonly ILogger<BaseParser> _logger;
    private readonly IHtmlDownloader _htmlDownloader;

    public BaseParser(ILogger<BaseParser> logger, IHtmlDownloader htmlDownloader)
    {
        _logger = logger;
        _htmlDownloader = htmlDownloader;
    }

    protected abstract string GetTitle(IDocument document);
    protected abstract string GetSeries(IDocument document);
    protected abstract int GetVolumeNumber(IDocument document);
    protected abstract string GetAuthors(IDocument document);
    protected abstract string GetCover(IDocument document);
    protected abstract DateTimeOffset? GetReleaseDate(IDocument document);
    protected abstract string GetISBN(IDocument document);
    protected abstract int GetTotalVolumes(IDocument document);
    protected abstract string? GetSeriesStatus(IDocument document);
    protected abstract string? GetOriginalSeriesName(IDocument document);
    protected abstract string GetPublisher(IDocument document);
    protected abstract DateTimeOffset? GetPublishDate(IDocument document);
    protected abstract string GetCountryCode(IDocument document);
    protected abstract bool GetIsPreorder(IDocument document);
    protected abstract Ownership.VolumeType GetBookType();
    protected abstract int? GetAgeRestriction(IDocument document);
    public abstract string SiteUrl { get; }

    

    public abstract string GetNextPageUrl();
    public abstract string GetVolumeUrlBlockClass();

    public async Task<IEnumerable<string>> GetVolumesUrls(string pageUrl)
    {
        var html = await _htmlDownloader.GetUrlHtml(pageUrl);
        var parser = new HtmlParser();
        var document = parser.ParseDocument(html);

        try
        {
            var classToSearch = GetVolumeUrlBlockClass();
            var nodes = document.QuerySelectorAll(classToSearch).Where(x=> !x.TextContent.ToLower().StartsWith("комплект"));
            var attribute = nodes.Select(x => x.Attributes["href"]);
            return attribute.Select(x => x.Value);
        }
        catch
        {
            _logger.LogTrace(html);
            throw;
        }
    }



    public async Task<ParsedInfo> Parse(string url)
    {
        var html = await _htmlDownloader.GetUrlHtml(url);
        var parser = new HtmlParser();
        var document = parser.ParseDocument(html);

        try
        {
            var title = GetTitle(document);
            var volumeNumber = GetVolumeNumber(document);
            var series = GetSeries(document);
            var cover = GetCover(document);
            var release = GetReleaseDate(document);
            var publisher = GetPublisher(document);
            var type = GetBookType();
            var isbn = GetISBN(document);
            var totalVol = GetTotalVolumes(document);
            var seriesStatus = GetSeriesStatus(document);
            var originalSeriesName = GetOriginalSeriesName(document);
            var authors = GetAuthors(document);
            var preorderDate = GetPublishDate(document);
            var countryCode = GetCountryCode(document);
            var isPreorder = GetIsPreorder(document);
            var ageRestriction = GetAgeRestriction(document);

            var parsed = new ParsedInfo(
                title, 
                authors, 
                volumeNumber, 
                series, 
                cover, 
                release, 
                publisher, 
                type.ToString(), 
                isbn, totalVol, 
                seriesStatus, 
                originalSeriesName, 
                url, 
                preorderDate, 
                countryCode, 
                isPreorder, 
                ageRestriction);

            var jsonOptions = new System.Text.Json.JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            parsed.json = System.Text.Json.JsonSerializer.Serialize(parsed, jsonOptions);
            _logger.LogDebug(parsed.json);

            return parsed;
        }
        catch (Exception ex)
        {
            var s = new StackTrace(ex);
            var assmbly = Assembly.GetAssembly(typeof(BaseParser));
            var methods = string.Empty;

            foreach (var frame in s.GetFrames()!)
            {
                var method = frame.GetMethod();
                if (method != null && method.Module.Assembly == assmbly)
                {
                    methods = method.Name;
                    break;
                }
            }

            _logger.LogWarning(methods);
            _logger.LogTrace(html);
            throw;
        }

    }
}