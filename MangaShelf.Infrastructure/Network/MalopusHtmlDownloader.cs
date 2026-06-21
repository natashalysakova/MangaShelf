using MangaShelf.BL.Contracts;
using Microsoft.Extensions.Logging;

namespace MangaShelf.Infrastructure.Network;

public class MalopusHtmlDownloader : AdvancedHtmlDownloader
{
    public MalopusHtmlDownloader(ILogger<BasicHtmlDownloader> logger, IConfigurationService configuration) : base(logger, configuration)
    {
    }
    
    // protected override async Task PreRequest(string url, CancellationToken token = default)
    // {
    //     var html = await this.GetHtmlFromUrl(url, token);
    //     if (html.Contains("defaultHash"))
    //     {
    //         var hashIndex = html.IndexOf("defaultHash");
    //         var hashStart = html.IndexOf("\"", hashIndex) + 1;
    //         var hashEnd = html.IndexOf("\"", hashStart);
    //         var hash = html.Substring(hashStart, hashEnd - hashStart);

    //         AddHeaders(new Dictionary<string, string>
    //         {
    //             { "Cookie", $"challenge_passed={hash}; max-age=1800; path=/; samesite=Lax" }
    //         });

    //         Thread.Sleep(300); // Wait for 0.3 seconds before making the next request
    //     }
    // }
}

