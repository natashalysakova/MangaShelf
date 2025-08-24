using MangaShelf.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace MangaShelf.SeedService;

public class SeedProdUsersService : ISeedDataService
{
    public SeedProdUsersService(ILogger<SeedProdUsersService> logger, IHostApplicationLifetime hostApplicationLifetime)
    {
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
        UserManager<ApplicationUser> userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var provider = "local";

        var adminUserName = "admin@example.com";
        if (await userManager.FindByLoginAsync(provider, adminUserName) is null)
        {
            var user = new ApplicationUser()
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
        if (await userManager.FindByLoginAsync(provider, demoUserName) is null)
        {
            var user = new ApplicationUser()
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
    }

}
