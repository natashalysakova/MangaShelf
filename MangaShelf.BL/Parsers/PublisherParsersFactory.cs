using MangaShelf.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MangaShelf.BL.Parsers;

public class PublisherParsersFactory
{
    private readonly IEnumerable<IPublisherParser> _parsers;

    public PublisherParsersFactory(IServiceProvider serviceProvider)
    {
        _parsers = serviceProvider.GetServices<IPublisherParser>();
    }

    public IEnumerable<IPublisherParser> GetParsers()
    {
        return _parsers;
    }

    public IPublisherParser? CreateParser(string url)
    {
        foreach (var parser in _parsers)
        {
            if (url.StartsWith(parser.SiteUrl))
            {
                return parser;
            }
        }

        return default;
    }
}
