using MangaShelf.BL.Contracts;

namespace MangaShelf.BL.Configuration;

public class HtmlDownloaderSettings : IConfigurationSection
{
    public TimeSpan RequestTimeout { get; set; }
    public int MaxRetries { get; set; }
    public TimeSpan DelayBetweenRetries { get; set; }
    public string BrowserWSEndpoint { get; set; }
}
