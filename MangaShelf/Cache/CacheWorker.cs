using MangaShelf.BL.Contracts;
using MangaShelf.BL.Services;
using MangaShelf.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace MangaShelf.Cache
{
    //public class CacheWorker : BackgroundService
    //{
    //    private readonly CacheSignal _cacheSignal;
    //    private readonly ILogger<CacheWorker> _logger;
    //    private readonly IServiceProvider _serviceProvider;

    //    public CacheWorker(CacheSignal cacheSignal, ILogger<CacheWorker> logger, IServiceProvider serviceProvider)
    //    {
    //        _cacheSignal = cacheSignal;
    //        _logger = logger;
    //        _serviceProvider = serviceProvider;
    //    }

    //    public override async Task StartAsync(CancellationToken cancellationToken)
    //    {
    //        await _cacheSignal.WaitAsync();
    //        await base.StartAsync(cancellationToken);
    //    }

    //    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    //    {
    //        await Task.Delay(2000, stoppingToken);
    //        _logger.LogInformation("Updating cache.");

    //        while (!stoppingToken.IsCancellationRequested)
    //        {
    //            using var scope = _serviceProvider.CreateScope();
    //            var cacheProvider = scope.ServiceProvider.GetRequiredService<ICacheProvider>();

    //            await cacheProvider.RebuildCache(stoppingToken);

    //            try
    //            {
    //                var options = scope.ServiceProvider.GetRequiredService<IConfigurationService>().CacheSettings;
    //                _logger.LogInformation(
    //                    "Will attempt to update the cache in {Hours} hours from now.",
    //                   options.UpdateInterval.TotalHours);

    //                await Task.Delay(options.UpdateInterval, stoppingToken);
    //            }
    //            catch (OperationCanceledException)
    //            {
    //                _logger.LogWarning("Cancellation acknowledged: shutting down.");
    //                break;
    //            }
    //        }
    //    }
    //}

    public class CacheSignal
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public Task WaitAsync() => _semaphore.WaitAsync();
        public Task<bool> TryWaitAsync(CancellationToken cancellationToken = default) => _semaphore.WaitAsync(0, cancellationToken);
        public void Release() => _semaphore.Release();
    }

    public interface ICacheProvider
    {
        Task RebuildCache(CancellationToken stoppingToken);
    }
    public class CacheProvider : ICacheProvider, ICacheInvalidator
    {
        private readonly IMemoryCache _cache;
        private readonly CacheSignal _cacheSignal;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CacheProvider> _logger;

        public CacheProvider(IMemoryCache cache, CacheSignal cacheSignal, IServiceProvider serviceProvider, ILogger<CacheProvider> logger)
        {
            _cache = cache;
            _cacheSignal = cacheSignal;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        public async Task RebuildCache(CancellationToken stoppingToken)
        {
            // Only one rebuild at a time; skip if already in progress
            if (!await _cacheSignal.TryWaitAsync(stoppingToken))
            {
                _logger.LogInformation("Cache rebuild already in progress, skipping.");
                return;
            }

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
                _cacheSignal.Release();
            }

        }
    }

    public static class CacheServiceInstaller
    {
        public static IHostApplicationBuilder RegisterCacheServices(this IHostApplicationBuilder builder)
        {
            // Cache services
            builder.Services.AddScoped<ICacheService, CacheService>();

            //builder.Services.AddHostedService<CacheWorker>();
            builder.Services.AddSingleton<CacheSignal>();
            builder.Services.AddSingleton<CacheProvider>();
            builder.Services.AddSingleton<ICacheProvider>(sp => sp.GetRequiredService<CacheProvider>());
            builder.Services.AddSingleton<ICacheInvalidator>(sp => sp.GetRequiredService<CacheProvider>());

            return builder;
        }
    }
}
