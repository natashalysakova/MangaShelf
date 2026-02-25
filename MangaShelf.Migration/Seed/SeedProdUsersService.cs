using MangaShelf.DAL.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
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
            await SeedAdmin(adminUserName);
        }
        else
        {
            await AddMissingAdminRoles(adminUser);
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

    private async Task AddMissingAdminRoles(MangaIdentityUser adminUser)
    {
        var hasUserRole = await _userManager.IsInRoleAsync(adminUser, RoleTypes.User);
        if (!hasUserRole)
        {
            await _userManager.AddToRoleAsync(adminUser, RoleTypes.User);
            _logger.LogInformation("Added missing {Role} role to existing admin user {UserName}", RoleTypes.User, adminUser.UserName);
        }
    }

    private async Task SeedAdmin(string adminUserName)
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
        await _userManager.AddToRoleAsync(user, RoleTypes.User);

        _logger.LogInformation("Seeded admin user with username {UserName}", adminUserName);
    }

    private async Task SeedRolesAsync()
    {
        var rolesToCreate = new List<string> { RoleTypes.Admin, RoleTypes.Cataloger, RoleTypes.User, RoleTypes.Service };

        foreach (var role in rolesToCreate)
        {
            var result = new IdentityRole(role);
            await _roleManager.CreateAsync(result);
            _logger.LogInformation("Created {Role} role", role);

        }
    }
}
