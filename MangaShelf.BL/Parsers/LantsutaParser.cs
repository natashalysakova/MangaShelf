
using AngleSharp.Dom;
using MangaShelf.DAL.Models;
using System.Text.RegularExpressions;

namespace MangaShelf.Parser.VolumeParsers;

class LantsutaParser() : BaseParser
{
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
        return GetFromTable(document, "Видавництво:");
    }

    protected override DateTime? GetReleaseDate(IDocument document)
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
        node = node.Replace(GetSeries(document), "");
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
        if(title.Contains("Том"))
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

        return 1;
    }

    public override string GetNextPageUrl()
    {
        throw new NotImplementedException();
    }

    public override string GetVolumeUrlBlockClass()
    {
        throw new NotImplementedException();
    }

    protected override DateTime? GetPublishDate(IDocument document)
    {
        throw new NotImplementedException();
    }

    protected override string GetCountryCode(IDocument document)
    {
        throw new NotImplementedException();
    }
}
