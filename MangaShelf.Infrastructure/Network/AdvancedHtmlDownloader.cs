using MangaShelf.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace MangaShelf.Infrastructure.Network;

public class AdvancedHtmlDownloader : IHtmlDownloader
{
    private readonly ILogger<BasicHtmlDownloader> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public AdvancedHtmlDownloader(ILogger<BasicHtmlDownloader> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36");
    }
    public async Task<string> GetUrlHtml(string url)
    {
        var maxretry = _configuration.GetSection("HtmlDownloaders").GetValue<int>("MaxRetries");
        int retry = 0;
        do
        {
            try
            {
                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();

                await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox" },
                });

                await using var page = await browser.NewPageAsync();
                await page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36");
                await page.SetExtraHttpHeadersAsync(new Dictionary<string, string>
                {
                    { "Accept-Language", "uk,uk-UA;q=0.9,en-US;q=0.8,en;q=0.7,de;q=0.6" }
                });

                await page.GoToAsync(url, WaitUntilNavigation.Networkidle0);
                var content = await page.GetContentAsync();
                return content;
            }
            catch (Exception ex)
            {
                await Task.Delay(1000);
                Console.WriteLine($"Error accessing {url}: {ex.Message}");
                Console.WriteLine("retry");
                retry += 1;
            }
        } while (retry < maxretry);

        throw new Exception("Cannot access website");
    }
}
