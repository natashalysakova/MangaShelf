using AngleSharp.Dom;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Parsers;

public class MolfarParser : BaseParser
{
    public MolfarParser(ILogger<BaseParser> logger, IHtmlDownloader htmlDownloader) : base(logger, htmlDownloader)
    {
    }

    public override string SiteUrl => "https://molfarpublishing.ua";

    public override string CatalogUrl => "/catalog/manga";

    public override string Pagination => "/page/{0}/";

    public override string VolumeTitleSelector => ".nameProduct";

    public override string GetPageUrl(int page)
    {
        if (page == 1)
        {
            return this.SiteUrl + this.CatalogUrl;
        }

        return base.GetPageUrl(page);
    }

    protected override int? GetAgeRestriction(IDocument document)
    {
        return null;
    }

    private static string? GetFromTable(IDocument document, string fieldName)
    {
        var rows = document.QuerySelectorAll("div.item");
        foreach (var row in rows)
        {
            var header = row.QuerySelector(".name");
            if (header != null && header.TextContent.Trim().Equals(fieldName, StringComparison.OrdinalIgnoreCase))
            {
                var value = row.QuerySelector(".value");
                if (value != null)
                {
                    return value.TextContent.Trim();
                }
            }
        }
        return null;
    }

    protected override string? GetAuthors(IDocument document)
    {
        var authors = GetFromTable(document, "Автор");
        var illustrators = GetFromTable(document, "Ілюстратори");

        authors = string.Join(";", new[] { authors, illustrators }.Where(x => !string.IsNullOrEmpty(x)));

        if (authors != null)
        {
            var splitted = authors.Split(new[] { ';', '&', '/' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            authors = string.Join(",", splitted);
        }

        return authors;
    }

    protected override VolumeType GetVolumeType(IDocument document)
    {
        var ePurchase = document.QuerySelector(".e-purchase");
        if (ePurchase != null && ePurchase.TextContent.Trim() != string.Empty)
        {
            return VolumeType.Digital | VolumeType.Physical;
        }

        return VolumeType.Physical;
    }

    protected override string GetCover(IDocument document)
    {
        var coverNode = document.QuerySelector(".wrapImg");
        if (coverNode is null)
            return string.Empty;

        return this.SiteUrl + coverNode.GetAttribute("src")!;
    }

    protected override string GetISBN(IDocument document)
    {
        return GetFromTable(document, "ISBN") ?? string.Empty;
    }

    protected override bool GetIsPreorder(IDocument document)
    {
        //var badgeNode = document.QuerySelector("span.bds.bgc7.tc1");
        //if (badgeNode is null)
        //    return false;

        //return badgeNode.TextContent.Trim().Equals("Передзамовлення", StringComparison.OrdinalIgnoreCase);

        var releseDateNode = document.QuerySelector(".productShortDescription");
        if (releseDateNode is null)
            return false;

        if(releseDateNode.TextContent.Contains("передзамовлення", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        return false;
    }

    protected override string? GetOriginalSeriesName(IDocument document)
    {
        return GetFromTable(document, "Оригінальна назва");
    }

    protected override DateTimeOffset? GetSaleStartDate(IDocument document)
    {
        return null;
    }

    protected override string GetPublisher(IDocument document)
    {
        return "Molfar Comics";
    }

    protected override DateTimeOffset? GetReleaseDate(IDocument document)
    {
        if (GetIsPreorder(document))
        {

            var releseDateNode = document.QuerySelector(".productShortDescription");
            if (releseDateNode is null)
                return null;

            var releseDateText = releseDateNode.TextContent.Trim();

            // Орієнтовна дата відправки передзамовлення 2 половина липня

            return GetDateFromText(releseDateText);
        }
        else
        {
            var year = GetFromTable(document, "Рік видання");
            return ParseYearIntoLastDayOfYear(year);
        }
    }

    private DateTimeOffset? GetDateFromText(string text)
    {
        text = text.Replace("Орієнтовна дата відправки передзамовлення", "").Trim();


        if (text.Contains('–') || text.Contains('-'))
        {
            var minus = text.Contains('–') ? '–' : '-';

            text = text.Substring(text.LastIndexOf(minus) + 1).Trim([' ', '.', '.', '!']);
        }

        text = text.ToUpper();

        string? month = default;
        int day = 0;

        if (text.StartsWith("1 ПОЛОВИНА"))
        {
            month = text.Substring("1 ПОЛОВИНА".Length);
            day = 15;
        }
        if (text.StartsWith("ПОЧАТОК"))
        {
            month = text.Substring("ПОЧАТОК".Length);
            day = 10;
        }

        if (text.StartsWith("2 ПОЛОВИНА"))
        {
            month = text.Substring("2 ПОЛОВИНА".Length);
        }
        if (text.StartsWith("КІНЕЦЬ"))
        {
            month = text.Substring("КІНЕЦЬ".Length);
        }

        if (StartsWithMonth(text))
        {
            if (text.Contains(' '))
            {
                month = text.Substring(0, text.IndexOf(' '));
            }
            else
            {
                month = text;
            }
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

    private int TryToGetFromDate(string text)
    {
        var split = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var potentialYear = split.Where(x => x.Length == 4);
        if (potentialYear.Any())
        {
            foreach (var item in potentialYear)
            {
                if (int.TryParse(item, out var year))
                {
                    return year;
                }
            }
        }

        return -1;
    }


    protected override SeriesStatus GetSeriesStatus(IDocument document)
    {
        return SeriesStatus.Unknown;
    }

    protected override string GetVolumeUrlBlockClass()
    {
        return "a.link";
    }

    protected override bool GetCanBePublished()
    {
        return true;
    }

    protected override SeriesType GetSeriesType(IDocument document)
    {
        return SeriesType.Unknown;
    }

    protected override string? GetDescription(IDocument document)
    {
        var descNode = document.QuerySelector(".desc");
        return descNode?.TextContent;
    }
}