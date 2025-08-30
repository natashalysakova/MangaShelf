using MangaShelf.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MangaShelf.Infrastructure.Network;

public class BasicHtmlDownloader : IHtmlDownloader
{
    private readonly ILogger<BasicHtmlDownloader> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public BasicHtmlDownloader(ILogger<BasicHtmlDownloader> logger, IConfiguration configuration)
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
                var page = await _httpClient.GetStringAsync(url);
                return page;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Retrying {url}\nPrevious run was failed: {error}", url, ex.Message);
                retry += 1;
                await Task.Delay(1000);
            }
        } while (retry < maxretry);

        throw new Exception("Cannot access website");
    }
}
