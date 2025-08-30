using MangaShelf.DAL;
using MangaShelf.DAL.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace MangaShelf.Infrastructure.Seed;

public class SeedProdUsersService : ISeedDataService
{
    private readonly ILogger<SeedProdUsersService> _logger;
    private readonly UserManager<MangaIdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public SeedProdUsersService(
        ILogger<SeedProdUsersService> logger, 
        UserManager<MangaIdentityUser> userManager, 
        RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _userManager = userManager;
        _roleManager = roleManager;
    }
    public async Task Run()
    {
        await Run(CancellationToken.None);
    }
    public string ActivitySourceName => "Seed prod users";

    public int Priority => 1;

    public async Task Run(CancellationToken cancellationToken)
    {
        await SeedRolesAsync();
        await SeedUsers();
    }

    private async Task SeedUsers()
    {
        var provider = "local";

        var adminUserName = "admin@example.com";
        var adminUser = await _userManager.FindByNameAsync(adminUserName);
        if (adminUser is null)
        {
            var user = new MangaIdentityUser()
            {
                UserName = adminUserName,
                Email = adminUserName,
                EmailConfirmed = true,
            };

            await _userManager.CreateAsync(user, "Admin@123");
            await _userManager.AddClaimAsync(user, new Claim(CustomClaimTypes.MustChangePassword, "true"));
            await _userManager.AddToRoleAsync(user, RoleTypes.Admin);
        }

        var demoUserName = "demo@example.com";
        var demoUser = await _userManager.FindByNameAsync(demoUserName);
        if (demoUser is null)
        {
            var user = new MangaIdentityUser()
            {
                UserName = demoUserName,
                Email = demoUserName,
                EmailConfirmed = true,
            };

            await _userManager.CreateAsync(user, "Demo@123");
            await _userManager.AddClaimAsync(user, new Claim(CustomClaimTypes.CannotChangePassword, "true"));
            await _userManager.AddClaimAsync(user, new Claim(CustomClaimTypes.IsDemoUser, "true"));
                  
            await _userManager.AddToRoleAsync(user, RoleTypes.User);
        }

        var parserServiceUserName = "PARSER_SERVICE";
        var parserServiceUser = await _userManager.FindByNameAsync(parserServiceUserName);
        if (parserServiceUser is null)
        {
            var user = new MangaIdentityUser()
            {
                UserName = parserServiceUserName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, "Tp!s2vDzpxB93*4V");
            if (result.Succeeded)
            {
                await _userManager.AddClaimAsync(user, new Claim(CustomClaimTypes.CannotChangePassword, "true"));
                await _userManager.AddToRoleAsync(user, RoleTypes.Service);
            }
        }
    }
    private async Task SeedRolesAsync()
    {
        if (!await _roleManager.RoleExistsAsync(RoleTypes.Admin))
        {
            var adminRole = new IdentityRole(RoleTypes.Admin);
            await _roleManager.CreateAsync(adminRole);
        }

        if (!await _roleManager.RoleExistsAsync(RoleTypes.Cataloger))
        {
            var catalogerRole = new IdentityRole(RoleTypes.Cataloger);
            await _roleManager.CreateAsync(catalogerRole);
        }

        if (!await _roleManager.RoleExistsAsync(RoleTypes.User))
        {
            var userRole = new IdentityRole(RoleTypes.User);
            await _roleManager.CreateAsync(userRole);
        }

        if (!await _roleManager.RoleExistsAsync(RoleTypes.Service))
        {
            var serviceRole = new IdentityRole(RoleTypes.Service);
            await _roleManager.CreateAsync(serviceRole);
        }
    }

}
