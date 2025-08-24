using MangaShelf.DAL;
using MangaShelf.DAL.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace MangaShelf.SeedService;

public class SeedDevUsersService : ISeedDataService
{
    public SeedDevUsersService(ILogger<SeedDevUsersService> logger)
    {
    }

    public string ActivitySourceName => "Seed dev users";

    public int Priority => 90;

    private async Task SeedUsersAsync(IServiceProvider serviceProvider)
    {
        UserManager<User> userManager = serviceProvider.GetRequiredService<UserManager<User>>();

        var provider = "local";

        var catalogerUserName = "cataloger@example.com";
        if (await userManager.FindByLoginAsync(provider, catalogerUserName) is null)
        {
            var user = new User()
            {
                UserName = catalogerUserName,
                Email = catalogerUserName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "Cataloger@123");

            await userManager.AddClaimAsync(user, new Claim(CustomClaimTypes.MustChangePassword, "true"));
            await userManager.AddToRoleAsync(user, RoleTypes.Cataloger);
        }
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
