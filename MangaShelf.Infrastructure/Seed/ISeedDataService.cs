namespace MangaShelf.Infrastructure.Seed;

public interface ISeedDataService
{
    string ActivitySourceName { get; }
    int Priority { get; }

    Task Run(CancellationToken cancellationToken);
    async Task Run()
    {
        await Run(CancellationToken.None);
    }
}