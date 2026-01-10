namespace MangaShelf.BL.Services;

public class JobManagerOptions
{

    /// <summary>
    /// Section name in appsettings.json
    /// </summary>
    public static string SectionName => "JobManager";

    /// <summary>
    /// Delay between job runs in hours.
    /// </summary>
    public int DelayBetweenRuns { get; set; }

    /// <summary>
    /// Maximum number of parsers to run in parallel.
    /// </summary>
    public int MaxParallelParsers { get; set; }
    public bool ResetNextRun { get; set; }
}