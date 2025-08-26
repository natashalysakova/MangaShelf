namespace MangaShelf.SeedService;

public interface ISeedDataService
{
    string ActivitySourceName { get; }
    int Priority { get; }

    Task Run(IServiceProvider scopedServiceProvider, CancellationToken cancellationToken);
    Task Run(IServiceProvider scopedServiceProvider);
}