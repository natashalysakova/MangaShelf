
using AngleSharp.Dom;
using MangaShelf.BL.Interfaces;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace MangaShelf.Cache
{
    public class CacheWorker : BackgroundService
    {
        private readonly ICacheService _cacheService;
        private readonly IMemoryCache _cache;
        private readonly CacheSignal _cacheSignal;
        private readonly ILogger<CacheWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly CacheOptions _options;
        private bool _isCacheInitialized = false;

        public CacheWorker(IMemoryCache cache, CacheSignal cacheSignal, ILogger<CacheWorker> logger, IOptions<CacheOptions> options,
            IServiceProvider serviceProvider)
        {
            _cache = cache;
            _cacheSignal = cacheSignal;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _options = options.Value;
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
                try
                {
                    using var scope = _serviceProvider.CreateScope();
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
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.AbsoluteExpiration)
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
                       _options.UpdateIntervalInHours);

                    await Task.Delay(_options.UpdateInterval, stoppingToken);
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
    public class CacheOptions
    {
        public bool Enabled { get; set; }
        public int AbsoluteExpiration { get; set; }

        private int updateInterval;
        /// <summary>
        /// In milliseconds
        /// </summary>
        public int UpdateInterval { get => updateInterval; set => updateInterval = value * 60 * 1000; }

        /// <summary>
        /// In hours
        /// </summary>
        public int UpdateIntervalInHours { get => UpdateInterval / 60 / 60 / 1000; }
        public static string SectionName { get => "Cache"; }
    }
}
