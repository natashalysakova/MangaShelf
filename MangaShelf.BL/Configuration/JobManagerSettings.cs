using MangaShelf.BL.Interfaces;

namespace MangaShelf.BL.Configuration;

public class JobManagerSettings : IConfigurationSection
{
    public TimeSpan DelayBetweenRuns { get; set; }
    public int MaxParallelParsers { get; set; }
    public bool ResetNextRun { get; set; }
}
