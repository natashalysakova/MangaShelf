using AngleSharp;
using AngleSharp.Dom;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Parsers.Malopus;
public class MalopusParser : BaseParser
{
    public override string SiteUrl => "https://malopus.com.ua";
    public override string CatalogUrl => "/manga/";
    public override string Pagination => "filter/page={0}/";

    private string? GetFromTable(IDocument document, string headerText)
    {
        var nodes = document.QuerySelectorAll(".product-features__row");

        foreach (var item in nodes)
        {
            var header = item.QuerySelector("th > span");

            if (header is not null && header.TextContent.Contains(headerText))
            {
                var value = item.QuerySelector("td");
                return value?.TextContent.Trim();
            }
        }

        return string.Empty;
    }

    protected override string? GetAuthors(IDocument document)
    {
        return GetFromTable(document, "Автор");
    }

    protected override string GetCover(IDocument document)
    {
        var node = document.QuerySelector(".gallery__photo-img");
        var attribute = node.Attributes["src"];
        return this.SiteUrl + attribute.Value;
    }

    protected override DateTimeOffset? GetReleaseDate(IDocument document)
    {
        var html = document.ToHtml();

        int index = html.IndexOf("Орієнтовна дата надходження:");
        if (index == -1)
            return null;


        var date = html.Substring(index + "Орієнтовна дата надходження:".Length + 1, 10);

        if (date == "0000-00-00")
            return null;

   
        if (DateTimeOffset.TryParseExact(date, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AssumeLocal, out DateTimeOffset parsedExactDate))
        {
            return parsedExactDate;
        }

        if (DateTimeOffset.TryParse(date, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal, out DateTimeOffset parsedDate))
        {
            return parsedDate;
        }

        return null;
    }

    protected override string GetSeries(IDocument document)
    {
        var nodes = document.QuerySelectorAll(".breadcrumbs-i");
        var seriesBreadcrumb = nodes.ElementAtOrDefault(3);
        if (seriesBreadcrumb is null)
            return GetVolumeTitle(document);


        return seriesBreadcrumb.TextContent.Replace("Манґа", "").Trim([' ', '\n']);
    }

    protected override string GetVolumeTitle(IDocument document)
    {
        var node = document.QuerySelector(".product-title");
        var title = node.InnerHtml.ToString();

        var lookupChar = new char[] { '.', '!', '?' };
        int index = -1;
        foreach (var ch in lookupChar)
        {
            index = node.InnerHtml.IndexOf(ch);
            if (index != -1)
            {
                break;
            }
        }

        if(index != -1)
        {
             title = title.Substring(index + 1).Trim();
        }

        if (title.StartsWith("Ранобе") || title.StartsWith("Манґа") || title.StartsWith("Комікс"))
        {
            title = title.Substring(title.IndexOf(' ') + 1).Trim();
        }

        return title;
    }

    string[] lookupArray = [". Том ", "! Том ", "? Том ", ". Омнібус ", "! Омнібус ", "? Омнібус "];

    public MalopusParser(ILogger<MalopusParser> logger,  IHtmlDownloader htmlDownloader) : base(logger, htmlDownloader)
    {
    }

    protected override int GetVolumeNumber(IDocument document)
    {
        var node = document.QuerySelector(".product-title");
        var title = node.InnerHtml;

        if (!lookupArray.Any(x => title.Contains(x)))
        {
            return -1;
        }

        var lookupValue = lookupArray.Single(x => title.Contains(x));

        int indexOfVolume, nextWhitespace;
        indexOfVolume = title.IndexOf(lookupValue) + lookupValue.Length;
        nextWhitespace = title.IndexOf(' ', indexOfVolume);
        string volume;
        if (nextWhitespace == -1)
        {
            volume = title.Substring(indexOfVolume).Trim();
        }
        else
        {
            volume = title.Substring(indexOfVolume, nextWhitespace - indexOfVolume);
        }


        //var volInd = title.IndexOf("Том ");

        //if (volInd == -1)
        //{
        //    volInd = title.IndexOf("Омнібус");

        //    if(volInd == -1)
        //        return volInd;
        //}

        //var nextWhiteSpace = title.IndexOf(' ', volInd);
        //string volume;
        //if (nextWhiteSpace == -1)
        //{
        //    volume = title.Substring(volInd + 1).Trim();
        //}
        //else
        //{
        //    volume = title.Substring(nextWhiteSpace).Trim();
        //}
        return int.Parse(volume);
    }

    protected override VolumeType GetVolumeType(IDocument document)
    {
        return VolumeType.Physical;
    }

    protected override string GetISBN(IDocument document)
    {
        return GetFromTable(document, "ISBN") ?? string.Empty;
    }

    protected override int GetTotalVolumes(IDocument document)
    {
        var text = GetFromTable(document, "Кількість томів");
        if (text is null)
            return -1;

        if (text.Contains('/'))
        {
            return int.Parse(text.Split('/', StringSplitOptions.RemoveEmptyEntries).First());
        }
        else if (text.Contains('(') && text.Contains(')'))
        {
            var indexopen = text.IndexOf('(') + 1;
            var indexclose = text.IndexOf(')');
            return int.Parse(text.Substring(indexopen, indexclose - indexopen));
        }
        else if (int.TryParse(text, out int totalVolumes))
        {
            return totalVolumes;
        }
        else
        {
            return GetVolumeNumber(document);
        }
    }

    protected override SeriesStatus GetSeriesStatus(IDocument document)
    {
        var text = GetFromTable(document, "Кількість томів");

        if (text.Contains("онґоїнґ"))
        {
            return SeriesStatus.Ongoing;
        }
        else if (text == "1")
        {
            return SeriesStatus.OneShot;
        }
        else
        {
            return SeriesStatus.Completed;
        }
    }

    protected override string? GetOriginalSeriesName(IDocument document)
    {
        return GetFromTable(document, "Оригінальна назва");
    }

    protected override string GetPublisher(IDocument document)
    {
        return "Mal'opus";
    }

    protected override string GetVolumeUrlBlockClass()
    {
        return ".catalogCard-title > a";
    }

    protected override DateTimeOffset? GetSaleStartDate(IDocument document)
    {
        // var newsDates = document.QuerySelectorAll(".rm-news-item-date");
        // var dates = new List<DateTimeOffset>();
        // foreach (var item in newsDates)
        // {
        //     if(DateTime.TryParseExact(item.InnerHtml, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal, out var parsedDate))
        //     {
        //         dates.Add(parsedDate);
        //     }
        // }

        // var earliest = dates.OrderBy(x => x).FirstOrDefault();
        // return earliest;
        return DateTimeOffset.Now;
    }
    protected override bool GetIsPreorder(IDocument document)
    {
        var lable = document.QuerySelector(".product-price__availability");

        if (lable is null)
            return false;

        return lable.TextContent.Contains("Передзамовлення");
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
        var node = document.QuerySelector(".text");
        return node?.TextContent;
    }
}
