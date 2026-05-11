using MangaShelf.Common.Interfaces;

namespace MangaShelf.Parser.Services;

public class NullCacheInvalidator : ICacheInvalidator
{
    private readonly ILogger<NullCacheInvalidator> _logger;

    public NullCacheInvalidator(ILogger<NullCacheInvalidator> logger)
    {
        _logger = logger;
    }

    public Task RebuildCache(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cache rebuild skipped (NullCacheInvalidator active).");
        return Task.CompletedTask;
    }
}
