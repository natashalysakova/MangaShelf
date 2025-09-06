using MangaShelf.BL.Parsers;

namespace MangaShelf.BL.Interfaces;

public interface IPublisherParser
{
    Task<ParsedInfo> Parse(string url, CancellationToken token = default);
    string GetNextPageUrl();
    Task<IEnumerable<string>> GetVolumesUrls(string url, CancellationToken token = default);
    bool CanParse(string url);

    string CatalogUrl { get; }
    string Pagination { get; }

    public string SiteUrl { get; }

    bool IsRunning { get; }

    public string ParserName { get; }
}
