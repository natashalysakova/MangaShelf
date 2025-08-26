using MangaShelf.DAL;
using MangaShelf.DAL.Identity;
using MangaShelf.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace MangaShelf.SeedService;

public class SeedDevUsersService : ISeedDataService
{
    public SeedDevUsersService(ILogger<SeedDevUsersService> logger)
    {
    }
    public async Task Run(IServiceProvider scopedServiceProvider)
    {
        await Run(scopedServiceProvider, CancellationToken.None);
    }

    public string ActivitySourceName => "Seed dev users";

    public int Priority => 90;

    private async Task SeedUsersAsync(IServiceProvider serviceProvider)
    {
        await Task.CompletedTask;
    }


    public async Task Run(IServiceProvider scopedServiceProvider, CancellationToken cancellationToken)
    {
        await SeedRolesAsync(scopedServiceProvider);
        await SeedUsersAsync(scopedServiceProvider);
    }

    private async Task SeedRolesAsync(IServiceProvider scopedServiceProvider)
    {
        await Task.CompletedTask;
    }
}
