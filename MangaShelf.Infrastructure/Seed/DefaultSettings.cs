using AngleSharp;
using MangaShelf.BL.Configuration;

namespace MangaShelf.Infrastructure.Seed;

public static class DefaultSettings
{
    public static IEnumerable<SeedSetting> Settings = new SettingBuilder()
        .Add<BackgroundWorkerSettings>(nameof(BackgroundWorkerSettings.Enabled), true)
        .Add<BackgroundWorkerSettings>(nameof(BackgroundWorkerSettings.StartDelay), TimeSpan.FromMinutes(1))
        .Add<BackgroundWorkerSettings>(nameof(BackgroundWorkerSettings.LoopDelay), TimeSpan.FromSeconds(3))
        .Add<JobManagerSettings>(nameof(JobManagerSettings.DelayBetweenRuns), TimeSpan.FromHours(12))
        .Add<JobManagerSettings>(nameof(JobManagerSettings.MaxParallelParsers), 3)
        .Add<JobManagerSettings>(nameof(JobManagerSettings.ResetNextRun), false)
        .Add<ParserServiceSettings>(nameof(ParserServiceSettings.DelayBetweenParse), TimeSpan.FromSeconds(5))
        .Add<ParserServiceSettings>(nameof(ParserServiceSettings.IgnoreExistingVolumes), true)
        .Add<HtmlDownloaderSettings>(nameof(HtmlDownloaderSettings.RequestTimeout), TimeSpan.FromSeconds(30))
        .Add<HtmlDownloaderSettings>(nameof(HtmlDownloaderSettings.MaxRetries), 3)
        .Add<HtmlDownloaderSettings>(nameof(HtmlDownloaderSettings.DelayBetweenRetries), TimeSpan.FromSeconds(5))
#if DEBUG
        .Add<HtmlDownloaderSettings>(nameof(HtmlDownloaderSettings.PuppetreeExecPath), "/usr/bin/google-chrome")
#else
        .Add<HtmlDownloaderSettings>(nameof(HtmlDownloaderSettings.PuppetreeExecPath), "/usr/bin/chromium")
#endif
        .Add<CacheSettings>(nameof(CacheSettings.Enabled), true)
        .Add<CacheSettings>(nameof(CacheSettings.AbsoluteExpiration), TimeSpan.FromHours(6))
        .Add<CacheSettings>(nameof(CacheSettings.UpdateInterval), TimeSpan.FromMinutes(1))
        .Build();
}