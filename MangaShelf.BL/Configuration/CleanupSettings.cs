using MangaShelf.BL.Contracts;

namespace MangaShelf.BL.Configuration;

public class CleanupSettings : IConfigurationSection
{
    public int CleanupOldJobsAfterDays { get; set; }
    public bool EnableJobCleanup { get; set; }
    public bool CleanupFailedJobs { get; set; }
    public TimeSpan CleanupInterval { get; set; }
}
