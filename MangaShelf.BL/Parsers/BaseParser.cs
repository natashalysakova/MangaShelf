using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using MangaShelf.BL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization;

namespace MangaShelf.BL.Parsers;

public abstract class BaseParser : IPublisherParser
{
    protected const int maxretry = 10;
    private readonly ILogger<BaseParser> _logger;

    public BaseParser(ILogger<BaseParser> logger)
    {
        _logger = logger;
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

    public abstract string SiteUrl { get; }

    protected virtual async Task<string> GetUrlHtml(string url)
    {
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Clear();

        //client.DefaultRequestHeaders.Add("Accept-language", "en-GB,en;q=0.9");
        //client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        //client.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
        //client.DefaultRequestHeaders.Add("Connection", "keep-alive");

        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36");
        int retry = 0;
        do
        {
            try
            {
                var page = await client.GetStringAsync(url);
                return page;
            }
            catch (Exception)
            {
                await Task.Delay(1000);
                Console.WriteLine("retry");
                retry += 1;
            }
        } while (retry < maxretry);

        throw new Exception("Cannot access website");
    }

    public abstract string GetNextPageUrl();
    public abstract string GetVolumeUrlBlockClass();

    public async Task<IEnumerable<string>> GetVolumesUrls(string pageUrl)
    {
        var html = await GetUrlHtml(pageUrl);
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
        var html = await GetUrlHtml(url);
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

            var parsed = new ParsedInfo(title, authors, volumeNumber, series, cover, release, publisher, type.ToString(), isbn, totalVol, seriesStatus, originalSeriesName, url, preorderDate, countryCode, isPreorder);

            var jsonOptions = new System.Text.Json.JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            _logger.LogDebug(System.Text.Json.JsonSerializer.Serialize(parsed, jsonOptions));
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
