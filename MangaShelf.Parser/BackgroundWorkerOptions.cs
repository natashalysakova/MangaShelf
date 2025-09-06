namespace MangaShelf.Parser;

/// <summary>
/// Represents configuration options for a background worker, including scheduling and behavior settings.
/// </summary>
public class BackgroundWorkerOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the worker is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Section name in appsettings.json
    /// </summary>
    public static string SectionName => "BackgroundWorker";

    private int _startDelay; // in milliseconds
    private int _loopDelay; // in milliseconds

    /// <summary>
    /// Worker start delay in seconds.
    /// </summary>
    public int StartDelay
    {
        get => _startDelay; 
        set => _startDelay = value * 1000;
    }

    /// <summary>
    /// Worker loop delay in seconds.
    /// </summary>
    public int LoopDelay
    {
        get => _loopDelay;
        set => _loopDelay = value * 1000;
    }
}
