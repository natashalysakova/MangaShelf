using MangaShelf.BL.Interfaces;

namespace MangaShelf.BL.Parsers;

public class ParserFactory : IParserFactory
{
    private readonly IEnumerable<IPublisherParser> _parsers;

    public ParserFactory(IEnumerable<IPublisherParser> parsers )
    {
        _parsers = parsers;
    }

    public IEnumerable<IPublisherParser> GetParsers()
    {
        return _parsers;
    }

    public IPublisherParser? GetParserForUrl(string url)
    {
        foreach (var parser in _parsers)
        {
            if (parser.CanParse(url))
            {
                return parser;
            }
        }
        return default;
    }
}
