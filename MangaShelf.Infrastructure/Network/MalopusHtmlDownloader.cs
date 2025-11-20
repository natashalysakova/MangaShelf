using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MangaShelf.Infrastructure.Network;

public class MalopusHtmlDownloader : BasicHtmlDownloader
{
    public MalopusHtmlDownloader(ILogger<BasicHtmlDownloader> logger, IConfiguration configuration) : base(logger, configuration)
    {
    }
    
    protected override async Task PreRequest(string url, CancellationToken token = default)
    {
        var html = await this.GetHtmlFromUrl(url, token);
        if (html.StartsWith("<script>"))
        {
            var hashIndex = html.IndexOf("defaultHash");
            var hashStart = html.IndexOf("\"", hashIndex) + 1;
            var hashEnd = html.IndexOf("\"", hashStart);
            var hash = html.Substring(hashStart, hashEnd - hashStart);

            AddHeaders(new Dictionary<string, string>
            {
                { "Cookie", $"challenge_passed={hash}; max-age=1800; path=/; samesite=Lax" }
            });
        }
    }
}

