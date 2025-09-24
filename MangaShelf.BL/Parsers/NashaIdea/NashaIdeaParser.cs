using AngleSharp.Dom;
using MangaShelf.BL.Enums;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Parsers.NashaIdea;

public class NashaIdeaParser : BaseParser
{

    public override string SiteUrl => "https://nashaidea.com/";

    public override string CatalogUrl => "product-category/manga/";

    public override string Pagination => "page/{0}";

    protected override string? GetAuthors(IDocument document)
    {
        return null;
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

        return GetSeriesNameFromDefaultTitle(title);
    }

    protected override string GetVolumeTitle(IDocument document)
    {
        var tag = document.QuerySelector("h1.product_title");
        var title = tag.InnerHtml;

        return GetVolumeTitleFromDefaultTitle(title);
    }

    protected override int GetVolumeNumber(IDocument document)
    {
        var tag = document.QuerySelector("h1.product_title");
        var title = tag.InnerHtml.ToLower();

        return GetVolumeNumberFromDefaultTitle(title);
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

        var year = TryToGetFromDate(text);
        if (year == -1)
        {
            year = DateTime.Today.Month > monthNumber.number ? DateTime.Today.Year + 1 : DateTime.Today.Year;

        }

        if (day == 0)
            day = DateTime.DaysInMonth(year, monthNumber.number);


        return DateTime.SpecifyKind(new DateTime(year, monthNumber.number, day), DateTimeKind.Local);
    }

    private int TryToGetFromDate(string text)
    {
        var split = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var potentialYear = split.Where(x => x.Length == 4);
        if(potentialYear.Any())
        {
            foreach (var item in potentialYear)
            {
                if(int.TryParse(item, out var year))
                {
                    return year;
                }
            }
        }

        return -1;
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

    protected override VolumeType GetVolumeType(IDocument document)
    {
        return VolumeType.Physical;
    }

    protected override string GetISBN(IDocument document)
    {
        // var node = document.QuerySelector(".book-product-table-ibn");
        // var text = node?.TextContent.Substring(node.TextContent.IndexOf(":") + 1).Trim();
        // return text;
        return string.Empty;
    }

    protected override int GetTotalVolumes(IDocument document)
    {
        var node = document.QuerySelector(".book-product-table-data-status");
        if (node is null)
            return -1;

        if (node.TextContent.Contains("Серія завершена"))
        {
            var volumeNumberNode = document.QuerySelector(".book-product-table-data-volumes > span");
            if (volumeNumberNode != null && int.TryParse(volumeNumberNode.TextContent.Trim(), out var volumeNumber))
            {
                return volumeNumber;
            }
        }

        var text = node.TextContent;

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

    protected override SeriesStatus GetSeriesStatus(IDocument document)
    {
        var node = document.QuerySelectorAll(".book-product-table-data-status");
        foreach (var item in node)
        {
            if (item.TextContent.Contains("Статус серії:"))
            {
                if (item.TextContent.Contains("Однотомник") || item.TextContent.Contains("Однотомна"))
                {
                    return SeriesStatus.OneShot;
                }

                if (item.TextContent.Contains("Серія незавершена") || item.TextContent.Contains("Серія не завершена"))
                {
                    return SeriesStatus.Ongoing;
                }

                return SeriesStatus.Completed;
            }
        }
        return SeriesStatus.Unknown;
    }

    protected override string? GetOriginalSeriesName(IDocument document)
    {
        return null;
    }

    protected override string GetPublisher(IDocument document)
    {
        return "Nasha Idea";
    }

    protected override DateTimeOffset? GetSaleStartDate(IDocument document)
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

    protected override string GetVolumeUrlBlockClass()
    {
        return ".woocommerce-loop-product__link";
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

    protected override string? GetDescription(IDocument document)
    {
        var node = document.QuerySelector(".book-product-description");
        return node?.TextContent;
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
