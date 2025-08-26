using MangaShelf.BL.Interfaces;

namespace MangaShelf.Parser.VolumeParsers;

public class PublisherParsersFactory
{
    private readonly IEnumerable<IPublisherParser> _parsers;

    public PublisherParsersFactory()
    {
        _parsers = [
            new NashaIdeaParser(),
            //new MalopusParser(),
            //new AmazonParser(),
            //new KoboParser(),
            //new LantsutaParser()
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
                parser.SetUrl(url);
                return parser;
            }
        }

        return default;
    }
}
