using MangaShelf.BL.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace MangaShelf.Cache
{
    public class CacheWorker : BackgroundService
    {
        private readonly ICacheService _cacheService;
        private readonly IMemoryCache _cache;
        private readonly CacheSignal _cacheSignal;
        private readonly ILogger<CacheWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private bool _isCacheInitialized = false;

        public CacheWorker(IMemoryCache cache, CacheSignal cacheSignal, ILogger<CacheWorker> logger, IServiceProvider serviceProvider)
        {
            _cache = cache;
            _cacheSignal = cacheSignal;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await _cacheSignal.WaitAsync();
            await base.StartAsync(cancellationToken);
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(2000, stoppingToken);
            _logger.LogInformation("Updating cache.");

            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var options = scope.ServiceProvider.GetRequiredService<IConfigurationService>().CacheSettings;
                
                try
                {
                    var _publisherService = scope.ServiceProvider.GetRequiredService<IPublisherService>();
                    var publishers = await _publisherService.GetAllNamesAsync(stoppingToken);

                    var _authorService = scope.ServiceProvider.GetRequiredService<IAuthorService>();
                    var authors = await _authorService.GetAllNamesAsync(stoppingToken);

                    var _volumeService = scope.ServiceProvider.GetRequiredService<IVolumeService>();
                    var volumes = await _volumeService.GetAllTitlesAsync(stoppingToken);

                    var _seriesService = scope.ServiceProvider.GetRequiredService<ISeriesService>();
                    var series = await _seriesService.GetAllTitlesAsync(stoppingToken);


                    var searchAutoComplete =
                        series
                        .Union(volumes)
                        .Union(authors)
                        .Union(publishers)
                        .Distinct();

                    var memoryCacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = options.AbsoluteExpiration
                    };

                    _cache.Set("Search", searchAutoComplete, memoryCacheOptions);
                    _logger.LogInformation(
                        "Cache updated with {Count:#,#} search autocomplete items.", searchAutoComplete.Count());
                }
                finally
                {
                    if (!_isCacheInitialized)
                    {
                        _cacheSignal.Release();
                        _isCacheInitialized = true;
                    }
                }

                try
                {
                    _logger.LogInformation(
                        "Will attempt to update the cache in {Hours} hours from now.",
                       options.UpdateInterval.TotalHours);

                    await Task.Delay(options.UpdateInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Cancellation acknowledged: shutting down.");
                    break;
                }
            }
        }
    }

    public class CacheSignal
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public Task WaitAsync() => _semaphore.WaitAsync();
        public void Release() => _semaphore.Release();
    }
}
