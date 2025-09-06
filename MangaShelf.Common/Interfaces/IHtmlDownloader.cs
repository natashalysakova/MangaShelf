namespace MangaShelf.Common.Interfaces;

public interface IHtmlDownloader
{
    Task<string> GetUrlHtml(string url, CancellationToken token = default);
}
