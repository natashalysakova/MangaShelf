using AngleSharp.Dom;

namespace MangaShelf.BL.Interfaces;

public interface IPublisherParser
{
    Task<ParsedInfo> Parse(string url);
    string GetNextPageUrl();
    Task<IEnumerable<string>> GetVolumesUrls(string url);
    string SiteUrl { get; }
    void SetUrl(string url);
    Task<ParsedInfo> Parse();
}

[Serializable]
public class DocumentParseException : Exception
{
    public IDocument Document { get; }
    public DocumentParseException(string selector, IDocument document) : base(selector)
    {
        Document = document;
    }
}
