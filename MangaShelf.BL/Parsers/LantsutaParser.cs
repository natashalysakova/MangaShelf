
using AngleSharp.Dom;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace MangaShelf.BL.Parsers;

public class LantsutaParser : BaseParser
{
    private readonly ILogger<LantsutaParser> _logger;

    public LantsutaParser(ILogger<LantsutaParser> logger,  IHtmlDownloader htmlDownloader) : base(logger, htmlDownloader)
    {
        _logger = logger;
    }

    public override string SiteUrl => "https://lantsuta-publishing.com/";

    public override string CatalogUrl => "manga-ua";

    public override string Pagination => "?page={0}";

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

    protected override string? GetAuthors(IDocument document)
    {
        return GetFromTable(document, "Автор");
    }

    protected override VolumeType GetVolumeType(IDocument document)
    {
        return VolumeType.Physical;
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
        if (publisher is null)
        {
            publisher = "LANTSUTA";
        }
        return publisher;
    }

    static string[] lookupPhrases = new string[]
        {
            "Видання на стадії виробництва і з'явиться у продажі",
            "Відправка з"
        };

    protected override DateTimeOffset? GetReleaseDate(IDocument document)
    {
        var description = document.QuerySelector("#tab-description");
        var text = description?.TextContent;
        if (text is null || !text.ContainsAny(lookupPhrases))
        {
            var year = GetFromTable(document, "Дата виходу");
            return ParseYearIntoLastDayOfYear(year);
        }

        return ParseDescription(text);
    }



    public static DateTimeOffset? ParseDescription(string text)
    {

        var seasonPhrases = new (string pattern, int month)[]
        {
            ("взимку", 2),
            ("навесні", 5),
            ("влітку", 8),
            ("восени", 11),
            ("наприкінці", 12),
            ("у середині", 6),
            ("на початку", 3),
            ("січні", 1),
            ("лютому", 2),
            ("березні", 3),
            ("квітні", 4),
            ("травні", 5),
            ("червні", 6),
            ("липні", 7),
            ("серпні", 8),
            ("вересні", 9),
            ("жовтні", 10),
            ("листопаді", 11),
            ("грудні", 12)
        };

        var monthPhrases = new (string pattern, int month)[]
        {
            ("січня", 1),
            ("лютого", 2),
            ("березня", 3),
            ("квітня", 4),
            ("травня", 5),
            ("червня", 6),
            ("липня", 7),
            ("серпня", 8),
            ("вересня", 9),
            ("жовтня", 10),
            ("листопада", 11),
            ("грудня", 12)
        };

        if (text.Contains(lookupPhrases[0]))
        {
            var startIndex = text.IndexOf(lookupPhrases[0]) + lookupPhrases[0].Length;

            var dateString = text.Substring(startIndex).Trim();
            foreach (var season in seasonPhrases)
            {
                if (dateString.Contains(season.pattern))
                {
                    var month = season.month;
                    var indexAfterSeason = dateString.IndexOf(season.pattern) + season.pattern.Length;
                    var yearString = dateString.Substring(indexAfterSeason, 5).Trim();
                    var year = int.Parse(yearString);
                    return new DateTimeOffset(DateTime.SpecifyKind(new DateTime(year, month, DateTime.DaysInMonth(year, month)), DateTimeKind.Local));
                }
            }
        }

        if (text.Contains(lookupPhrases[1]))
        {
            var startIndex = text.IndexOf(lookupPhrases[1]) + lookupPhrases[1].Length;
            var dateString = text.Substring(startIndex).Trim();
            foreach (var monthPhrase in monthPhrases)
            {
                if (dateString.Contains(monthPhrase.pattern))
                {
                    var month = monthPhrase.month;
                    var indexAfterMonth = dateString.IndexOf(monthPhrase.pattern) + monthPhrase.pattern.Length;
                    var yearString = dateString.Substring(indexAfterMonth, 5).Trim();
                    var dayString = dateString.Substring(0, dateString.IndexOf(monthPhrase.pattern)).Trim();
                    var day = int.Parse(dayString);
                    var year = int.Parse(yearString);
                    return new DateTimeOffset(DateTime.SpecifyKind(new DateTime(year, month, day), DateTimeKind.Local));
                }
            }
        }


        return null;
    }

    protected override string GetSeries(IDocument document)
    {
        return GetFromTable(document, "Серія");
    }

    protected override SeriesStatus GetSeriesStatus(IDocument document)
    {
        return SeriesStatus.Unknown;
    }

    protected override string GetVolumeTitle(IDocument document)
    {
        var node = document.QuerySelector(".name-product-title").InnerHtml;
        var series = GetSeries(document);
        node = node.Replace(series, "").Trim();
        if (series.Contains('.'))
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

    protected override int GetVolumeNumber(IDocument document)
    {
        var title = GetVolumeTitle(document);
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

    protected override string GetVolumeUrlBlockClass()
    {
        return ".name-product > a";
    }

    protected override DateTimeOffset? GetSaleStartDate(IDocument document)
    {
        return null;
    }

    protected override bool GetIsPreorder(IDocument document)
    {
        var button = document.QuerySelector(".btn-buy");

        if (button is null)
        {
            return false;
        }

        return button.TextContent.Contains("Передзамовлення");
    }

    protected override int? GetAgeRestriction(IDocument document)
    {
        var ageString = GetFromTable(document, "Вік");
        if (ageString is not null && ageString.Contains("+"))
        {
            ageString = ageString.Replace("+", "").Trim();
        }
    
        if (int.TryParse(ageString, out var age))
        {
            return age;
        }

        return null;
    }

    protected override string? GetDescription(IDocument document)
    {
        var descriptionNode = document.QuerySelectorAll("#tab-description p");
        return string.Join("\n\n", descriptionNode.Select(n => n.TextContent.Trim()).Where(t => !string.IsNullOrWhiteSpace(t)));
    }
}

public static class StringExtensions
{
    public static bool ContainsAny(this string source, IEnumerable<string> toCheck)
    {
        foreach (var check in toCheck)
        {
            if (source.Contains(check))
            {
                return true;
            }
        }
        return false;
    }
}
