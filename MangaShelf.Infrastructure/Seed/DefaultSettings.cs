using MangaShelf.DAL.System.Models;

namespace MangaShelf.Infrastructure.Seed;

public static class DefaultSettings
{
    public static IEnumerable<SeedSetting> Settings => new List<SeedSetting>
    {
        new("BackgroundWorker", "Enabled", "true", SettingType.Bool),
        new("BackgroundWorker", "StartDelay", "00:01:00", SettingType.TimeSpan),
        new("BackgroundWorker", "LoopDelay", "00:00:03", SettingType.TimeSpan),

        new("JobManager", "DelayBetweenRuns", "12:00:00", SettingType.TimeSpan),
        new("JobManager", "MaxParallelParsers", "3", SettingType.Int),
        new("JobManager", "ResetNextRun", "false", SettingType.Bool),

        new("ParserService", "DelayBetweenParse", "00:00:05", SettingType.TimeSpan),
        new("ParserService", "IgnoreExistingVolumes", "true", SettingType.Bool),

        new("HtmlDownloader", "RequestTimeout", "00:00:30", SettingType.TimeSpan),
        new("HtmlDownloader", "MaxRetries", "3", SettingType.Int),
        new("HtmlDownloader", "DelayBetweenRetries", "00:00:05", SettingType.TimeSpan),

        new("Cache", "Enabled", "true", SettingType.Bool),
        new("Cache", "AbsoluteExpiration", "06:00:00", SettingType.TimeSpan),
        new("Cache", "UpdateInterval", "00:01:00", SettingType.TimeSpan),
    };

    public record SeedSetting(string Section, string Key, string Value, SettingType type);
}
