using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;
using MangaShelf.BL.Enums;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.DependencyInjection;
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

        string approxDateString = "Орієнтовна дата надходження:";

        int index = html.IndexOf(approxDateString);
        if (index == -1)
            return null;

        int nextWhitespace = html.IndexOf(' ', index + approxDateString.Length + 1);
        int length = nextWhitespace - (index + approxDateString.Length);
        if(length != 10)
        {
            nextWhitespace = html.IndexOf(' ', nextWhitespace + 1);
            length = nextWhitespace - (index + approxDateString.Length);
        }

        var date = html.Substring(index + approxDateString.Length + 1, length).Trim();

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

        if (LookupList.Any(x => date.Contains(x.season)))
        {
            return GetDateFromText(date);
        }

        return null;
    }

    private DateTimeOffset? GetDateFromText(string date)
    {
        int day;
        int month;
        int year;

        var splitted = date.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if(splitted.Length == 2)
        {
            month = LookupList.Single(x => x.season == splitted[0]).month;
            year = int.Parse(splitted[1]);
            day = DateTime.DaysInMonth(year, month);

            if(year == DateTime.Now.Year && month == 2 && month < DateTime.Now.Month)
            {
                month = 12; // fallback for winter. If year is current and month in past then it's till december current year
            }
            return new DateTimeOffset(year, month, day, 0, 0, 0, TimeZoneInfo.Local.GetUtcOffset(new DateTime(year, month, day)));
        }

        return null;
    }

    private (string season, int month)[] LookupList = [
        ("осінь", 11),
        ("літо", 8),
        ("зима", 2),
        ("весна", 5)];


    protected override string GetSeries(IDocument document)
    {
        var node = document.QuerySelector(".product-title");

        var title = node.TextContent;
        title = ReplaceVolumeType(title);

        var lookupChar = new char[] { '.', '!', '?' };
        int index = -1;
        foreach (var ch in lookupChar)
        {
            index = title.IndexOf(ch);
            if (index != -1)
            {
                break;
            }
        }

        if (index != -1)
        {
            title = title.Substring(0, index).Trim();
        }

        return title;
    }

    private static string ReplaceVolumeType(string title)
    {
        if (title.StartsWith("Ранобе") || title.StartsWith("Манґа") || title.StartsWith("Комікс") || title.StartsWith("Передзамовлення"))
        {
            title = title.Substring(title.IndexOf(' ') + 1).Trim();
        }

        return title;
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

        if (index != -1)
        {
            title = title.Substring(index + 1).Trim();
        }

        title = ReplaceVolumeType(title);

        return title;
    }

    string[] lookupArray = [". Том ", "! Том ", "? Том ", ". Омнібус ", "! Омнібус ", "? Омнібус "];

    public MalopusParser(ILogger<MalopusParser> logger, [FromKeyedServices(HtmlDownloaderKeys.Malopus)] IHtmlDownloader htmlDownloader) : base(logger, htmlDownloader)
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
        return null;
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
        // Get the description container
        var descriptionDiv = document.QuerySelector(".product-description .text");

        if (descriptionDiv == null)
            return null;

        var paragraphs = descriptionDiv.QuerySelectorAll("p");

        var description = string.Join("\n",
            paragraphs
                .Select(p => p.TextContent.Trim())
                .Where(text => !string.IsNullOrWhiteSpace(text))
        );

        if (description.Length == 0)
            return descriptionDiv.TextContent.Trim();

        return description;
    }
}
