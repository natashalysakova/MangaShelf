using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using MangaShelf.BL.Contracts;
using MangaShelf.Common.Helpers;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;

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

    protected virtual string GetSeries(IDocument document)
    {
        var parsedHeader = ParseHeader(document);
        return parsedHeader.Series;
    }

    protected virtual string GetVolumeTitle(IDocument document)
    {
        var parsedHeader = ParseHeader(document);
        return parsedHeader.Title;
    }

    protected virtual int? GetVolumeNumber(IDocument document)
    {
        var parsedHeader = ParseHeader(document);
        return parsedHeader.Number;
    }

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

    protected virtual int? GetTotalVolumes(IDocument document)
    {
        return null;
    }



    public abstract string SiteUrl { get; }

    public abstract string CatalogUrl { get; }

    public abstract string Pagination { get; }

    public bool IsRunning => _isRunning;

    public string ParserName { get => this.GetType().Name; }

    private bool _isRunning = false;

    public virtual string GetPageUrl(int page)
    {
        return $"{SiteUrl}{CatalogUrl}{Pagination.Replace("{0}", page.ToString())}";
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
            var nodes = document.QuerySelectorAll(classToSearch).Where(x => !x.TextContent.ToLower().StartsWith("комплект"));
            if (!nodes.Any())
            {
                throw new Exception("Page does not contain any volumes. Probably last page reached.");
            }
            var attribute = nodes.Where(x=> x.Attributes["href"] != null).Select(x => x.Attributes["href"]);
            return attribute.Select(x => x.Value);
        }
        catch
        {
            _logger.LogTrace(html);
            throw;
        }
    }

    private static DateTimeOffset? EnsureLocalOffset(DateTimeOffset? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        var dateTime = value.Value.DateTime;
        var offset = TimeZoneInfo.Local.GetUtcOffset(dateTime);
        return new DateTimeOffset(dateTime, offset);
    }

    public async Task<ParsedInfo> Parse(string url, string html, CancellationToken token)
    {
        _isRunning = true;

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
                Release = EnsureLocalOffset(GetReleaseDate(document)),
                Publisher = GetPublisher(document),
                VolumeType = GetVolumeType(document),
                Isbn = VolumeHelper.NormalizedIsbn(GetISBN(document)),
                TotalVolumes = GetTotalVolumes(document),
                SeriesStatus = GetSeriesStatus(document),
                OriginalSeriesTitle = GetOriginalSeriesName(document),
                Url = url,
                PreorderStartDate = EnsureLocalOffset(GetSaleStartDate(document)),
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

    public async Task<ParsedInfo> Parse(string url, CancellationToken token = default)
    {
        _isRunning = true;

        if (!url.StartsWith(SiteUrl))
        {
            url = SiteUrl + url;
        }

        var html = await _htmlDownloader.GetUrlHtml(url, token);

        return await Parse(url, html, token);
    }


    protected static DateTimeOffset? ParseYearIntoLastDayOfYear(string? year)
    {
        if (year != null && int.TryParse(year, out var yearNumber))
        {
            var month = 12;
            var day = 31;

            var date = new DateTime(yearNumber, month, day);
            var offset = TimeZoneInfo.Local.GetUtcOffset(date);

            return new DateTimeOffset(date, offset);
        }

        return null;
    }

    private string GetVolumeTitleFromDefaultTitle(string title)
    {
        var volIndex = LookupIndex(title);

        if (volIndex == -1)
        {
            return title.Trim();
        }
        else
        {
            return title.Substring(volIndex).Trim();
        }
    }

    private int LookupIndex(string input)
    {
        var volIndex = input.ToLower().IndexOf("омнібус");

        if (volIndex == -1)
            volIndex = input.ToLower().IndexOf("том");

        if (volIndex == -1)
        {
            volIndex = input.ToLower().LastIndexOf("книга");
        }

        return volIndex;
    }

    private int? GetVolumeNumberFromDefaultTitle(string title)
    {
        var volIndex = LookupIndex(title);
        if (volIndex == -1)
            return null;


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

        return int.TryParse(volume, out var n) ? n : null;
    }

    private string GetSeriesNameFromDefaultTitle(string title)
    {
        var volIndex = LookupIndex(title);
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

    public abstract string VolumeTitleSelector { get; }
    protected (string Series, string Title, int? Number) ParseHeader(IDocument document)
    {
        var node = document.QuerySelector(VolumeTitleSelector);
        var text = node?.TextContent?.Trim();

        var title = GetVolumeTitleFromDefaultTitle(text);
        var series = GetSeriesNameFromDefaultTitle(text);
        var number = GetVolumeNumberFromDefaultTitle(text);

        return (series, title, number);
    }

    public bool CanParse(string url)
    {
        return url.StartsWith(SiteUrl, StringComparison.OrdinalIgnoreCase);
    }
}
