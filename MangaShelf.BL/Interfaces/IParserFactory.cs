namespace MangaShelf.BL.Interfaces;

public interface IParserFactory
{
    IEnumerable<IPublisherParser> GetParsers();
    IPublisherParser? GetParserForUrl(string url);
}
