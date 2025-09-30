using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using MangaShelf.BL.Interfaces;
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

    protected abstract string GetVolumeTitle(IDocument document);
    protected abstract string GetSeries(IDocument document);
    protected abstract int GetVolumeNumber(IDocument document);
    protected abstract string? GetAuthors(IDocument document);
    protected abstract string GetCover(IDocument document);
    protected abstract DateTimeOffset? GetReleaseDate(IDocument document);
    protected abstract string? GetISBN(IDocument document);
    protected abstract SeriesStatus GetSeriesStatus(IDocument document);
    protected abstract string? GetOriginalSeriesName(IDocument document);
    protected abstract string GetPublisher(IDocument document);
    protected abstract DateTimeOffset? GetSaleStartDate(IDocument document);
    protected abstract bool GetIsPreorder(IDocument document);
    protected abstract VolumeType GetVolumeType(IDocument document);
    protected abstract int? GetAgeRestriction(IDocument document);
    protected abstract string GetVolumeUrlBlockClass();
    protected abstract string? GetDescription(IDocument document);

    protected virtual SeriesType GetSeriesType(IDocument document)
    {
        return SeriesType.Manga;
    }

    protected virtual string GetCountryCode(IDocument document)
    {
        return "ua";
    }

    protected virtual int GetTotalVolumes(IDocument document)
    {
        return -1;
    }



    public abstract string SiteUrl { get; }

    public abstract string CatalogUrl { get; }

    public abstract string Pagination { get; }

    public bool IsRunning => _isRunning;

    public string ParserName { get => this.GetType().Name; }

    private bool _isRunning = false;

    public virtual string GetNextPageUrl()
    {
        return $"{SiteUrl}{CatalogUrl}{Pagination}";
    }

    protected virtual bool GetCanBePublished()
    {
        return true;
    }

    public async Task<IEnumerable<string>> GetVolumesUrls(string pageUrl, CancellationToken token)
    {
        var html = await _htmlDownloader.GetUrlHtml(pageUrl, token);
        var parser = new HtmlParser();
        var document = await parser.ParseDocumentAsync(html, token);

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



    public async Task<ParsedInfo> Parse(string url, CancellationToken token = default)
    {
        _isRunning = true;

        if (!url.StartsWith(SiteUrl))
        {
            url = SiteUrl + url;
        }

        var html = await _htmlDownloader.GetUrlHtml(url, token);
        var parser = new HtmlParser();
        var document = await parser.ParseDocumentAsync(html, token);

        try
        {
            var parsed = new ParsedInfo
            {
                Title = GetVolumeTitle(document),
                Authors = GetAuthors(document),
                VolumeNumber = GetVolumeNumber(document),
                Series = GetSeries(document),
                Cover = GetCover(document),
                Release = GetReleaseDate(document),
                Publisher = GetPublisher(document),
                VolumeType = GetVolumeType(document),
                Isbn = GetISBN(document),
                TotalVolumes = GetTotalVolumes(document),
                SeriesStatus = GetSeriesStatus(document),
                OriginalSeriesName = GetOriginalSeriesName(document),
                Url = url,
                PreorderStartDate = GetSaleStartDate(document),
                CountryCode = GetCountryCode(document),
                IsPreorder = GetIsPreorder(document),
                AgeRestrictions = GetAgeRestriction(document),
                CanBePublished = GetCanBePublished(),
                SeriesType = GetSeriesType(document),
                Description = GetDescription(document)
            };

            var jsonOptions = new System.Text.Json.JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            parsed.Json = System.Text.Json.JsonSerializer.Serialize(parsed, jsonOptions);
            _logger.LogDebug(parsed.Json);

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
        finally
        {
            _isRunning = false;
        }
        }
    protected static DateTimeOffset? ParseYearIntoLastDayOfYear(string? year)
    {
        if (year != null && int.TryParse(year, out var yearNumber))
        {
            var month = 12;
            var day = 31;

            var date = DateTime.SpecifyKind(new DateTime(yearNumber, month, day), DateTimeKind.Local);

            return new DateTimeOffset(date);
        }

        return null;
    }

    protected string GetVolumeTitleFromDefaultTitle(string title)
    {
        var volIndex = title.ToLower().LastIndexOf("том");
        if (volIndex == -1)
        {
            return title.Trim();
        }
        else
        {
            return title.Substring(volIndex).Trim();
        }
    }

    protected int GetVolumeNumberFromDefaultTitle(string title)
    {
        var volIndex = title.ToLower().IndexOf("том");

        if (volIndex == -1)
            return volIndex;

        var nextWord = title.IndexOf(" ", volIndex + 3);
        if (nextWord == -1)
        {
            nextWord = volIndex + 3;
        }

        var nextWhitespace = title.IndexOf(" ", nextWord + 1);

        string volume;
        if (nextWhitespace == -1)
        {
            volume = title.Substring(nextWord).Trim();
        }
        else
        {
            volume = title.Substring(nextWord, nextWhitespace - nextWord).Trim();
        }

        return int.Parse(volume);
    }

    protected string GetSeriesNameFromDefaultTitle(string title)
    {
        var volIndex = title.ToLower().LastIndexOf("том");
        if (volIndex == -1)
        {
            return title.Trim();
        }
        else
        {
            var series = title.Substring(0, volIndex).Trim([' ', ',', '.']);
            return series;
        }
    }

    public bool CanParse(string url)
    {
        return url.StartsWith(SiteUrl, StringComparison.OrdinalIgnoreCase);
    }
}
