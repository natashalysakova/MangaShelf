using MangaShelf.Common;

namespace MangaShelf.Common.Interfaces;

public interface IPublisherParser
{
    Task<ParsedInfo> Parse(string url);
    string GetNextPageUrl();
    Task<IEnumerable<string>> GetVolumesUrls(string url);
    string SiteUrl { get; }
}
