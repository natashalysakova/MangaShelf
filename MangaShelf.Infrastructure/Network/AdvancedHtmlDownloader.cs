using MangaShelf.BL.Configuration;
using MangaShelf.BL.Interfaces;
using MangaShelf.Common.Interfaces;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace MangaShelf.Infrastructure.Network;

public class AdvancedHtmlDownloader : IHtmlDownloader
{
    private readonly ILogger<BasicHtmlDownloader> _logger;
    private readonly HtmlDownloaderSettings _options;
    private readonly HttpClient _httpClient;

    public AdvancedHtmlDownloader(ILogger<BasicHtmlDownloader> logger, IConfigurationService configurationService)
    {
        _logger = logger;
        _options = configurationService.HtmlDownloader;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36");
        _httpClient.Timeout = _options.RequestTimeout;
    }
    public async Task<string> GetUrlHtml(string url, CancellationToken token = default)
    {
        var executablePath = Environment.GetEnvironmentVariable("PUPPETEER_EXECUTABLE_PATH");

        if (string.IsNullOrEmpty(executablePath) || !File.Exists(executablePath))
        {
            Environment.SetEnvironmentVariable("PUPPETEER_EXECUTABLE_PATH", null);
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            executablePath = null;
        }

        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            ExecutablePath = executablePath,
            Args = new[]
            {
                "--disable-gpu",
                "--disable-dev-shm-usage",
                "--disable-setuid-sandbox",
                "--no-sandbox"
            },
        });

        var maxretry = _options.MaxRetries;
        int retry = 0;
        do
        {
            try
            {
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
                Console.WriteLine($"Error accessing {url}: {ex.Message}");
                Console.WriteLine("retry");
                retry += 1;
                await Task.Delay(_options.DelayBetweenRetries, token);
            }
        } while (retry < maxretry);

        throw new Exception("Cannot access website");
    }
}