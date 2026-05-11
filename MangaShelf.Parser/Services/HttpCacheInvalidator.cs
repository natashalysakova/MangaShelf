using MangaShelf.Common.Interfaces;

namespace MangaShelf.Parser.Services;

public class HttpCacheInvalidator : ICacheInvalidator
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpCacheInvalidator> _logger;

    public HttpCacheInvalidator(HttpClient httpClient, ILogger<HttpCacheInvalidator> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task RebuildCache(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsync("/api/cache", null, cancellationToken);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("Cache rebuild triggered successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to trigger cache rebuild on web app.");
        }
    }
}
