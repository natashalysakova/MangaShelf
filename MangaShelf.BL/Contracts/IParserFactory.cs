namespace MangaShelf.BL.Contracts;

public interface IParserFactory
{
    IEnumerable<IPublisherParser> GetParsers();
    IPublisherParser? GetParserForUrl(string url);
}
