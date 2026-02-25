using MangaShelf.BL.Configuration;
using MangaShelf.DAL.System.Models;

namespace MangaShelf.BL.Interfaces;

public interface IConfigurationService
{
    public BackgroundWorkerSettings BackgroundWorker { get; }
    public JobManagerSettings JobManager { get; }
    public ParserServiceSettings ParserService { get; }
    public HtmlDownloaderSettings HtmlDownloader { get; }
    public CacheSettings CacheSettings { get; }
    public void InvalidateSection<TSection>() where TSection : class, IConfigurationSection, new();
    public Task<Settings> UpdateSectionValueAsync(Settings settings, CancellationToken token = default);
}
