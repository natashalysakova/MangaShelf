using AngleSharp;
using AngleSharp.Dom;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Parsers;

public class MalopusParser : BaseParser
{
    public override string SiteUrl => "https://malopus.com.ua/";
    private string _catalogUrl = "manga/";
    private string _pagination = "?page={0}";
    int currentPage = 0;

    protected override string GetAuthors(IDocument document)
    {
        var nodes = document.QuerySelectorAll(".rm-product-attr-list-item");

        foreach (var item in nodes)
        {
            var divs = item.ChildNodes.Where(x => x.NodeName == "DIV");
            var node = divs.FirstOrDefault(x => x.TextContent == "Автор");

            if (node is null)
            {
                continue;
            }

            return divs.Last().TextContent;
        }

        return string.Empty;
    }

    protected override string GetCover(IDocument document)
    {
        //var node = document.QuerySelector(".rm-product-title > h1");
        //var title = node.InnerHtml;

        //HttpClient client = new HttpClient();

        //var api = _configuration.GetValue<string>("googleApiKey");

        //var query = $"https://www.googleapis.com/customsearch/v1?cx=76a6d7a5dfbdf4163&key={api}&q={title}&searchType=image";


        //var task = client.GetAsync(query);
        //task.Wait();

        //if (task.IsCompleted && task.Result.IsSuccessStatusCode)
        //{
        //    var result = task.Result.Content.ReadAsStringAsync();
        //    result.Wait();
        //    var json = result.Result;
        //}
        //return string.Empty;



        var node = document.QuerySelector(".oct-gallery > img.img-fluid");
        var attribute = node.Attributes["src"];
        return attribute.Value;
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
        var nodes = document.QuerySelectorAll(".rm-product-center-info-item-title");
        var node = nodes.FirstOrDefault(x => x.InnerHtml == "Серія:");
        if (node is null)
            return GetTitle(document);


        return node.NextElementSibling.TextContent.Trim([' ', '\n']);
    }

    protected override string GetTitle(IDocument document)
    {
        var node = document.QuerySelector(".rm-product-title > h1");
        var title = node.InnerHtml.Substring(node.InnerHtml.LastIndexOf('.') + 1).Trim();

        if (title.StartsWith("Ранобе") || title.StartsWith("Манґа") || title.StartsWith("Комікс"))
        {
            title = title.Substring(title.IndexOf(' ') + 1).Trim();
        }

        return title;
    }

    string[] lookupArray = [". Том ", ". Омнібус "];

    public MalopusParser(ILogger<MalopusParser> logger) : base(logger)
    {
    }

    protected override int GetVolumeNumber(IDocument document)
    {
        var node = document.QuerySelector(".rm-product-title > h1");
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

    protected override Ownership.VolumeType GetBookType()
    {
        return Ownership.VolumeType.Physical;
    }

    protected override string GetISBN(IDocument document)
    {
        var nodes = document.QuerySelectorAll(".rm-product-tabs-attributtes-list-item");

        foreach (var node in nodes)
        {
            if (node.Children[0].TextContent.Contains("ISBN"))
            {
                return node.Children[1].TextContent.Trim();
            }
        }

        return string.Empty;
    }

    protected override int GetTotalVolumes(IDocument document)
    {
        var nodes = document.QuerySelectorAll(".rm-product-tabs-attributtes-list-item");

        foreach (var node in nodes)
        {
            if (node.Children[0].TextContent.Contains("Кількість томів"))
            {
                var text = node.Children[1].TextContent;

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
        }

        return -1;
    }

    protected override string? GetSeriesStatus(IDocument document)
    {
        var nodes = document.QuerySelectorAll(".rm-product-tabs-attributtes-list-item");

        foreach (var node in nodes)
        {
            if (node.Children[0].TextContent.Contains("Кількість томів"))
            {
                var text = node.Children[1].TextContent;

                if (text.Contains("онґоїнґ"))
                {
                    return "ongoing";
                }
                else if (text == "1")
                {
                    return "oneshot";
                }
                else
                {
                    return "finished";
                }
            }
        }

        return null;
    }

    protected override string? GetOriginalSeriesName(IDocument document)
    {
        var nodes = document.QuerySelectorAll(".rm-product-tabs-attributtes-list-item");

        foreach (var node in nodes)
        {
            if (node.Children[0].TextContent.Contains("Оригінальна назва"))
            {
                return node.Children[1].TextContent.Trim();
            }
        }

        return string.Empty;
    }

    protected override string GetPublisher(IDocument document)
    {
        return "Mal'opus";
    }

    public override string GetNextPageUrl()
    {
        return $"{SiteUrl}{_catalogUrl}{string.Format(_pagination, ++currentPage)}";
    }
    public override string GetVolumeUrlBlockClass()
    {
        return ".rm-module-title > a";
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
        var lable = document.QuerySelector(".rm-module-stock");

        if (lable is null)
            return false;

        return lable.TextContent.Contains("Попереднє замовлення");
    }
}
