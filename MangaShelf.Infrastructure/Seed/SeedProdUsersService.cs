using MangaShelf.DAL;
using MangaShelf.DAL.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace MangaShelf.Infrastructure.Seed;

public class SeedProdUsersService : ISeedDataService
{
    public SeedProdUsersService(ILogger<SeedProdUsersService> logger)
    {
    }
    public async Task Run(IServiceProvider scopedServiceProvider)
    {
        await Run(scopedServiceProvider, CancellationToken.None);
    }
    public string ActivitySourceName => "Seed prod users";

    public int Priority => 1;

    public async Task Run(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        await SeedRolesAsync(serviceProvider);
        await SeedUsers(serviceProvider);
    }

    private async Task SeedUsers(IServiceProvider serviceProvider)
    {
        UserManager<MangaIdentityUser> userManager = serviceProvider.GetRequiredService<UserManager<MangaIdentityUser>>();

        var provider = "local";

        var adminUserName = "admin@example.com";
        var adminUser = await userManager.FindByNameAsync(adminUserName);
        if (adminUser is null)
        {
            var user = new MangaIdentityUser()
            {
                UserName = adminUserName,
                Email = adminUserName,
                EmailConfirmed = true,
            };

            await userManager.CreateAsync(user, "Admin@123");
            await userManager.AddClaimAsync(user, new Claim(CustomClaimTypes.MustChangePassword, "true"));
            await userManager.AddToRoleAsync(user, RoleTypes.Admin);
        }

        var demoUserName = "demo@example.com";
        var demoUser = await userManager.FindByNameAsync(demoUserName);
        if (demoUser is null)
        {
            var user = new MangaIdentityUser()
            {
                UserName = demoUserName,
                Email = demoUserName,
                EmailConfirmed = true,
            };

            await userManager.CreateAsync(user, "Demo@123");
            await userManager.AddClaimAsync(user, new Claim(CustomClaimTypes.CannotChangePassword, "true"));
            await userManager.AddClaimAsync(user, new Claim(CustomClaimTypes.IsDemoUser, "true"));

            await userManager.AddToRoleAsync(user, RoleTypes.User);
        }

        var parserServiceUserName = "PARSER_SERVICE";
        var parserServiceUser = await userManager.FindByNameAsync(parserServiceUserName);
        if (parserServiceUser is null)
        {
            var user = new MangaIdentityUser()
            {
                UserName = parserServiceUserName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "Tp!s2vDzpxB93*4V");
            if (result.Succeeded)
            {
                await userManager.AddClaimAsync(user, new Claim(CustomClaimTypes.CannotChangePassword, "true"));
                await userManager.AddToRoleAsync(user, RoleTypes.Service);
            }
        }
    }
    private async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync(RoleTypes.Admin))
        {
            var adminRole = new IdentityRole(RoleTypes.Admin);
            await roleManager.CreateAsync(adminRole);
        }

        if (!await roleManager.RoleExistsAsync(RoleTypes.Cataloger))
        {
            var catalogerRole = new IdentityRole(RoleTypes.Cataloger);
            await roleManager.CreateAsync(catalogerRole);
        }

        if (!await roleManager.RoleExistsAsync(RoleTypes.User))
        {
            var userRole = new IdentityRole(RoleTypes.User);
            await roleManager.CreateAsync(userRole);
        }

        if (!await roleManager.RoleExistsAsync(RoleTypes.Service))
        {
            var serviceRole = new IdentityRole(RoleTypes.Service);
            await roleManager.CreateAsync(serviceRole);
        }
    }

}
