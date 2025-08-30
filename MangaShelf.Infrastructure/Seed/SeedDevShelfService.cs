using Microsoft.Extensions.Logging;

namespace MangaShelf.Infrastructure.Seed;

public class SeedDevShelfService : ISeedDataService
{
    public SeedDevShelfService(ILogger<SeedDevShelfService> logger)
    {
    }

    public string ActivitySourceName => "Seed dev shelf";

    public int Priority => 91;

    public async Task Run(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    public async Task Run()
    {
        await Run(CancellationToken.None);
    }
}
