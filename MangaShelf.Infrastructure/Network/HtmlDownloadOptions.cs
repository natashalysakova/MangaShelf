namespace MangaShelf.Infrastructure.Network;

/// <summary>
/// Options for HTML downloading
/// </summary>
public class HtmlDownloadOptions
{
    private int _requestTimeout;
    private int _delayBetweenRetries;

    public static string SectionName { get => "HtmlDownloaders"; }

    /// <summary>
    /// Timeout for each request in seconds.
    /// </summary>
    public int RequestTimeout
    {
        get => _requestTimeout;
        set => _requestTimeout = value * 1000;
    }
    public int MaxRetries { get; set; }

    /// <summary>
    /// Delay between retries in seconds.
    /// </summary>
    public int DelayBetweenRetries
    {
        get => _delayBetweenRetries; 
        set => _delayBetweenRetries = value * 1000;
    }
}