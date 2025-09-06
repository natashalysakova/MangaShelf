using Microsoft.Extensions.Logging;

namespace MangaShelf.Infrastructure.Seed;

public class SeedDevUsersService : ISeedDataService
{
    public SeedDevUsersService(ILogger<SeedDevUsersService> logger)
    {
    }
    public async Task Run()
    {
        await Run(CancellationToken.None);
    }

    public string ActivitySourceName => "Seed dev users";

    public int Priority => 90;

    private async Task SeedUsersAsync()
    {
        await Task.CompletedTask;
    }


    public async Task Run(CancellationToken cancellationToken)
    {
        await SeedRolesAsync();
        await SeedUsersAsync();
    }

    private async Task SeedRolesAsync()
    {
        await Task.CompletedTask;
    }
}
