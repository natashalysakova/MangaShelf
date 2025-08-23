using MangaShelf.DAL.MangaShelf;

namespace MangaShelf.SeedService;

public class SeedDevShelfService : ISeedDataService
{
    public SeedDevShelfService(ILogger<SeedDevShelfService> logger) 
    {
    }

    public string ActivitySourceName => "Seed dev shelf";

    public int Priority => 91;

    public async Task Run(IServiceProvider scopedServiceProvider, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
