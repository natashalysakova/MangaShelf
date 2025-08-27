using MangaShelf.BL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Parsers;

public class PublisherParsersFactory
{
    private readonly IEnumerable<IPublisherParser> _parsers;

    public PublisherParsersFactory(ILoggerFactory loggerFactory)
    {
        _parsers = [
            new NashaIdeaParser(loggerFactory.CreateLogger<NashaIdeaParser>()),
            new MalopusParser(loggerFactory.CreateLogger<MalopusParser>()),
            //new AmazonParser(loggerFactory.CreateLogger<AmazonParser>()),
            //new KoboParser(loggerFactory.CreateLogger<KoboParser>()),
            new LantsutaParser(loggerFactory.CreateLogger<LantsutaParser>())
            ];
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
