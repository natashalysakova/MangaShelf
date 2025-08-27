
using AngleSharp.Dom;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace MangaShelf.BL.Parsers;

public class LantsutaParser : BaseParser
{
    private readonly ILogger<LantsutaParser> _logger;

    public LantsutaParser(ILogger<LantsutaParser> logger) : base(logger)
    {
        _logger = logger;
    }

    public override string SiteUrl => "https://lantsuta-publishing.com/";

    private string? GetFromTable(IDocument document, string header)
    {
        var infoTable = document.QuerySelectorAll(".table--product-attributes tr");

        for (int i = 0; i < infoTable.Count(); i++)
        {
            if (infoTable[i].QuerySelector("th").InnerHtml.ToLower() == header.ToLower())
            {
                var htmlContent = infoTable[i].QuerySelector("td").InnerHtml;
                return Regex.Replace(htmlContent, "<.*?>", string.Empty).Trim();
            }
        }

        return null;
    }

    protected override string GetAuthors(IDocument document)
    {
        return GetFromTable(document, "Автор");
    }

    protected override Ownership.VolumeType GetBookType()
    {
        return Ownership.VolumeType.Physical;
    }

    protected override string GetCover(IDocument document)
    {
        var node = document.QuerySelector(".image-set__image > a > img");
        var attribute = node.Attributes["src"];
        return attribute.Value;
    }

    protected override string GetISBN(IDocument document)
    {
        return GetFromTable(document, "ISBN");
    }

    protected override string? GetOriginalSeriesName(IDocument document)
    {
        return null;
    }

    protected override string GetPublisher(IDocument document)
    {
        var publisher = GetFromTable(document, "Видавництво:");
        if(publisher is null)
        {
            publisher = "LANTSUTA";
        }
        return publisher;
    }

    protected override DateTimeOffset? GetReleaseDate(IDocument document)
    {
        return null;
    }

    protected override string GetSeries(IDocument document)
    {
        return GetFromTable(document, "Серія");
    }

    protected override string? GetSeriesStatus(IDocument document)
    {
        return null;
    }

    protected override string GetTitle(IDocument document)
    {
        var node = document.QuerySelector(".name-product-title").InnerHtml;
        var series = GetSeries(document);
        node = node.Replace(series, "").Trim();
        if(series.Contains('.'))
        {
            series = series.Replace('.', ':');
            node = node.Replace(series, "").Trim();
        }
        

        if (char.IsPunctuation(node[0]))
        {
            node = node.Substring(1).Trim();
        }
        return node;
    }

    protected override int GetTotalVolumes(IDocument document)
    {
        return 0;
    }

    protected override int GetVolumeNumber(IDocument document)
    {
        var title = GetTitle(document);
        if (title.Contains("Том"))
        {
            var volIndex = title.IndexOf("Том");
            var nextWord = title.IndexOf(" ", volIndex + 3);
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

        return -1;
    }

    private string _catalogUrl = "manga-ua";
    private string _pagination = "?page={0}";
    int currentPage = 0;

    public override string GetNextPageUrl()
    {
        return $"{SiteUrl}{_catalogUrl}{string.Format(_pagination, ++currentPage)}";
    }

    public override string GetVolumeUrlBlockClass()
    {
        return ".name-product > a";
    }

    protected override DateTimeOffset? GetPublishDate(IDocument document)
    {
        return null;
    }

    protected override string GetCountryCode(IDocument document)
    {
        return "UK";
    }

    protected override bool GetIsPreorder(IDocument document)
    {
        var button = document.QuerySelector(".btn-buy");

        if(button is null)
        {
            return false;
        }

        return button.TextContent.Contains("Передзамовлення");
    }
}
