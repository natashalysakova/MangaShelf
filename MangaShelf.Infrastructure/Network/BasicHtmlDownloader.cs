using MangaShelf.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MangaShelf.Infrastructure.Network;

public class BasicHtmlDownloader : IHtmlDownloader
{
    private readonly ILogger<BasicHtmlDownloader> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    protected virtual Task PreRequest(string url, CancellationToken token = default)
    {
        // Override in derived classes if needed
        return Task.CompletedTask;
    }
    protected void AddHeaders(Dictionary<string, string>? headers = null)
    {
        if(headers != null)
        {
            foreach (var header in headers)
            {
                if (_httpClient.DefaultRequestHeaders.Contains(header.Key))
                {
                    _httpClient.DefaultRequestHeaders.Remove(header.Key);
                }
                _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }
    }

    public BasicHtmlDownloader(ILogger<BasicHtmlDownloader> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36");

    }
    public async Task<string> GetUrlHtml(string url, CancellationToken token = default)
    {
        var maxretry = _configuration.GetSection("HtmlDownloaders").GetValue<int>("MaxRetries");
        int retry = 0;
        do
        {
            try
            {
                await PreRequest(url, token);
                return await GetHtmlFromUrl(url, token);
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogWarning("Retrying {url}\nPrevious run was failed: {error}", url, httpEx.StatusCode);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Retrying {url}\nPrevious run was failed: {error}", url, ex.Message);
                retry += 1;
                await Task.Delay(1000, token);
            }
        } while (retry < maxretry);

        throw new Exception("Cannot access website");
    }

    protected async Task<string> GetHtmlFromUrl(string url, CancellationToken token)
    {
        return await _httpClient.GetStringAsync(url, token);
    }
}
