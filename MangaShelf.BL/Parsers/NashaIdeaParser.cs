using AngleSharp.Dom;
using MangaShelf.BL.Enums;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Parsers;

public class NashaIdeaParser : BaseParser
{

    public override string SiteUrl => "https://nashaidea.com/";

    private string _catalogUrl = "product-category/manga/";
    private string _pagination = "page/{0}";

    protected override string GetAuthors(IDocument document)
    {
        return string.Empty;
    }

    protected override string GetCover(IDocument document)
    {
        var node = document.QuerySelectorAll(".woocommerce-product-gallery__image > a").First();
        var attribute = node.Attributes["href"];
        return attribute.Value;
    }

    protected override string GetSeries(IDocument document)
    {
        var tag = document.QuerySelector("h1.product_title");
        var title = tag.InnerHtml;

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

    protected override string GetTitle(IDocument document)
    {
        var tag = document.QuerySelector("h1.product_title");
        var title = tag.InnerHtml;

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

    protected override int GetVolumeNumber(IDocument document)
    {
        var tag = document.QuerySelector("h1.product_title");
        var title = tag.InnerHtml.ToLower();

        var volIndex = title.IndexOf("том");

        if (volIndex == -1)
            return volIndex;

        var nextWord = title.IndexOf(" ", volIndex + 3);
        if(nextWord == -1)
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

    protected override DateTimeOffset? GetReleaseDate(IDocument document)
    {
        var nodes = document.QuerySelectorAll(".book-product-description > p");
        string? releaseNode = null;
        foreach (var node in nodes)
        {
            if (node.TextContent.StartsWith("У НАЯВНОСТІ"))
            {
                releaseNode = node.TextContent;
            }
        }

        if (releaseNode is null)
            return null;

        var date = releaseNode.Replace("У НАЯВНОСТІ", "").Trim([' ', '.', '.', '!', ':']);
        var dateString = GetDateFromText(date);

        return dateString;
    }

    private DateTimeOffset? GetPreorderDate(string dateText)
    {
        var date = dateText.Replace("Дата публікації:", "").Trim([' ', '.', '.', '!']);

        var dateSplits = date.Split([' ', ',', '.', '–'], StringSplitOptions.RemoveEmptyEntries);

        if (dateSplits.Length != 3)
        {
            return null;
        }

        var day = int.Parse(dateSplits[0]);
        var month = monthes.Where(x => x.names.Contains(dateSplits[1].ToUpper())).FirstOrDefault().number;
        var year = int.Parse(dateSplits[2]);

        return DateTime.SpecifyKind(new DateTime(year, month, day), DateTimeKind.Local);
    }

    private DateTimeOffset? GetDateFromText(string text)
    {
        if (text.Contains('–'))
        {
            text = text.Substring(text.LastIndexOf('–') + 1).Trim([' ', '.', '.', '!']);
        }

        string? month = default;
        int day = 0;
        if (text.StartsWith("У "))
        {
            month = text.Substring(2);
        }
        if (text.StartsWith("ПЕРША ПОЛОВИНА"))
        {
            month = text.Substring("ПЕРША ПОЛОВИНА".Length);
            day = 15;
        }
        if (text.StartsWith("ДРУГА ПОЛОВИНА"))
        {
            month = text.Substring("ДРУГА ПОЛОВИНА".Length);
        }
        if (StartsWithMonth(text))
        {
            month = text.Substring(0, text.IndexOf(' '));
        }

        if (month == default)
            return null;

        month = month.Trim([' ', '.', '.', '!']);
        var monthNumber = monthes.SingleOrDefault(x => x.names.Contains(month));

        if (monthNumber == default)
            return null;

        var year = DateTime.Today.Month > monthNumber.number ? DateTime.Today.Year + 1 : DateTime.Today.Year;

        if (day == 0)
            day = DateTime.DaysInMonth(year, monthNumber.number);


        return DateTime.SpecifyKind(new DateTime(year, monthNumber.number, day), DateTimeKind.Local);
    }

    private bool StartsWithMonth(string text)
    {
        var splitted = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var month in monthes)
        {
            foreach (var name in month.names)
            {
                if (splitted[0].ToUpper() == name)
                {
                    return true;
                }
            }
        }
        return false;
    }

    protected override Ownership.VolumeType GetBookType()
    {
        return Ownership.VolumeType.Physical;
    }

    protected override string GetISBN(IDocument document)
    {
        var node = document.QuerySelector(".book-product-table-ibn");
        var text = node.TextContent.Substring(node.TextContent.IndexOf(":") + 1).Trim();
        return text;
    }

    protected override int GetTotalVolumes(IDocument document)
    {
        var nodes = document.QuerySelectorAll(".book-product-table-data-weight");


        var oldNode = nodes.SingleOrDefault(x => x.TextContent.Contains("Статус серії"));
        if (oldNode == null)
            return -1;

        var text = oldNode.TextContent;

        if (text.Contains("Однотомник") || text.Contains("Однотомна"))
        {
            return 1;
        }

        if (text.Contains("наразі"))
        {
            return GetFromStatus(text, "наразі");
        }

        if (text.Contains("Серія з "))
        {
            return GetFromStatus(text, "Серія з ");
        }

        var volumeNode = nodes.SingleOrDefault(x => x.TextContent.Contains("Кількість томів"));
        if (volumeNode != null)
        {
            var volumeText = volumeNode.TextContent;
            var index = volumeText.IndexOf(":");
            if (index != -1)
            {
                var numberPart = volumeText.Substring(index + 1).Trim();
                if (int.TryParse(numberPart, out var volumeCount))
                {
                    return volumeCount;
                }
            }
        }

        return -1;
    }

    private static int GetFromStatus(string text, string pattern)
    {
        var index = text.IndexOf(pattern);

        if (index != -1)
        {
            // Get the substring after "наразі"
            var afterText = text.Substring(index + pattern.Length);

            // Extract digits
            string numberStr = "";
            foreach (var c in afterText)
            {
                if (char.IsDigit(c))
                    numberStr += c;
                else if (!string.IsNullOrEmpty(numberStr))
                    break; // Stop after we've collected digits and hit a non-digit
            }

            if (!string.IsNullOrEmpty(numberStr) && int.TryParse(numberStr, out var volume))
            {
                return volume;
            }
        }
        return 0;
    }

    protected override string? GetSeriesStatus(IDocument document)
    {
        var node = document.QuerySelectorAll(".book-product-table-data-weight");
        foreach (var item in node)
        {
            if (item.TextContent.Contains("Статус серії:"))
            {
                if (item.TextContent.Contains("Однотомник") || item.TextContent.Contains("Однотомна"))
                {
                    return "oneshot";
                }

                if (item.TextContent.Contains("Серія незавершена") || item.TextContent.Contains("Серія не завершена"))
                {
                    return "ongoing";
                }

                return "finished";
            }
        }
        return null;
    }

    protected override string? GetOriginalSeriesName(IDocument document)
    {
        return null;
    }

    protected override string GetPublisher(IDocument document)
    {
        return "Nasha Idea";
    }

    protected override DateTimeOffset? GetPublishDate(IDocument document)
    {
        var nodes = document.QuerySelectorAll(".book-product-table-data-view");
        string? publishDate = null;
        foreach (var node in nodes)
        {
            if (node.TextContent.StartsWith("Дата публікації"))
            {
                publishDate = node.TextContent;
            }
        }

        if (publishDate is null)
            return null;

        var date = GetPreorderDate(publishDate);

        return date;
    }

    public override string GetNextPageUrl()
    {
        return $"{SiteUrl}{_catalogUrl}{_pagination}";
    }

    public override string GetVolumeUrlBlockClass()
    {
        return ".woocommerce-loop-product__link";
    }

    protected override string GetCountryCode(IDocument document)
    {
        return "UK";
    }

    protected override bool GetIsPreorder(IDocument document)
    {
        var lable = document.QuerySelector(".inventory_status");
        if (lable is null)
            return false;

        return lable.TextContent.Contains("Передзамовлення");
    }

    protected override int? GetAgeRestriction(IDocument document)
    {
        var nodes = document.QuerySelectorAll(".book-product-table-data-bik");
        string? ageString = null;
        foreach (var node in nodes)
        {
            if (node.TextContent.StartsWith("Вік:"))
            {
                ageString = node.TextContent.Replace("Вік:", string.Empty).Trim();
            }
        }

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

    (int number, string[] names)[] monthes = [
        (1, ["СІЧЕНЬ", "СІЧНІ", "СІЧНЯ"]),
        (2, ["ЛЮТИЙ", "ЛЮТОМУ", "ЛЮТОГО"]),
        (3, ["БЕРЕЗЕНЬ", "БЕРЕЗНІ", "БЕРЕЗНЯ"]),
        (4, ["КВІТЕНЬ", "КВІТНІ", "КВІТНЯ"]),
        (5, ["ТРАВЕНЬ", "ТРАВНІ", "ТРАВНЯ"]),
        (6, ["ЧЕРВЕНЬ", "ЧЕВНІ", "ЧЕРВНЯ"]),
        (7, ["ЛИПЕНЬ", "ЛИПНІ", "ЛИПНЯ"]),
        (8, ["СЕРПЕНЬ", "СЕРПНІ", "СЕРПНЯ"]),
        (9, ["ВЕРЕСЕНЬ", "ВЕРЕСНІ", "ВЕРЕСНЯ"]),
        (10, ["ЖОВТЕНЬ", "ЖОВТНІ", "ЖОВТНЯ"]),
        (11, ["ЛИСТОПАД", "ЛИСТОПАДІ", "ЛИСТОПАДА"]),
        (12, ["ГРУДЕНЬ", "ГРУДНІ", "ГРУДНЯ"]),
        ];


    public NashaIdeaParser(ILogger<NashaIdeaParser> logger, [FromKeyedServices(HtmlDownloaderKeys.Advanced)] IHtmlDownloader htmlDownloader) : base(logger, htmlDownloader)
    {
    }
}
