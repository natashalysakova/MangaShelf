using MangaShelf.BL.Contracts;
using MangaShelf.BL.Dto;
using MangaShelf.DAL;
using MangaShelf.DAL.Identity;
using MangaShelf.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.BL.Services;

public class UserService : IUserService
{
    private readonly IDbContextFactory<MangaDbContext> dbContextFactory;
    private readonly IDbContextFactory<MangaIdentityDbContext> identityDbContextFactory;
    private readonly UserManager<MangaIdentityUser> userManager;

    private static readonly string[] KnownRoles =
    [
        RoleTypes.Admin,
        RoleTypes.Cataloger,
        RoleTypes.User,
        RoleTypes.Service
    ];

    public UserService(
        IDbContextFactory<MangaDbContext> dbContextFactory,
        IDbContextFactory<MangaIdentityDbContext> identityDbContextFactory,
        UserManager<MangaIdentityUser> userManager)
    {
        this.dbContextFactory = dbContextFactory;
        this.identityDbContextFactory = identityDbContextFactory;
        this.userManager = userManager;
    }

    public async Task<string?> GetVisibleNameAsync(string identityUserId)
    {
        var context = dbContextFactory.CreateDbContext();
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
        return user?.VisibleUsername;
    }

    public async Task<User> RegisterShelfUserAsync(MangaIdentityUser user)
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        var shelfUser = new User
        {
            IdentityUserId = user.Id,
            VisibleUsername = user.UserName
        };
        dbContext.Users.Add(shelfUser);
        await dbContext.SaveChangesAsync();
        return shelfUser;
    }

    public async Task<User> UpdateVisibleNameAsync(string identityUserId, string visibleName)
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }
        user.VisibleUsername = visibleName;
        await dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<IReadOnlyCollection<AdminUserDto>> GetAllUsersWithRolesAsync()
    {
        using var shelfDbContext = dbContextFactory.CreateDbContext();
        using var identityDbContext = identityDbContextFactory.CreateDbContext();

        var shelfUsers = await shelfDbContext.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .ToListAsync();

        var visibleNames = shelfUsers.ToDictionary(u => u.IdentityUserId, u => u.VisibleUsername);
        var deletedStates = shelfUsers.ToDictionary(u => u.IdentityUserId, u => u.IsDeleted);

        var users = await identityDbContext.Users
            .AsNoTracking()
            .OrderBy(u => u.UserName)
            .ToListAsync();

        var rolesByUserId = await (
            from userRole in identityDbContext.UserRoles.AsNoTracking()
            join role in identityDbContext.Roles.AsNoTracking() on userRole.RoleId equals role.Id
            select new { userRole.UserId, role.Name }
        )
            .Where(x => x.Name != null)
            .GroupBy(x => x.UserId)
            .ToDictionaryAsync(
                group => group.Key,
                group => group.Select(x => x.Name!).Distinct().OrderBy(x => x).ToList());

        return users.Select(user => new AdminUserDto
        {
            IdentityUserId = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            VisibleName = visibleNames.GetValueOrDefault(user.Id),
            IsDeleted = deletedStates.GetValueOrDefault(user.Id),
            Roles = rolesByUserId.TryGetValue(user.Id, out var roles) ? roles : []
        }).ToList();
    }

    public async Task<IReadOnlyCollection<string>> UpdateUserRolesAsync(string identityUserId, IEnumerable<string> roles)
    {
        var user = await userManager.FindByIdAsync(identityUserId)
            ?? throw new InvalidOperationException("User not found.");

        var targetRoles = roles
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role.Trim())
            .Where(IsKnownRole)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var currentRoles = await userManager.GetRolesAsync(user);

        var rolesToRemove = currentRoles
            .Except(targetRoles, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (rolesToRemove.Count > 0)
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                throw BuildIdentityOperationException(removeResult, "Failed to remove roles.");
            }
        }

        var rolesToAdd = targetRoles
            .Except(currentRoles, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (rolesToAdd.Count > 0)
        {
            var addResult = await userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
            {
                throw BuildIdentityOperationException(addResult, "Failed to add roles.");
            }
        }

        return (await userManager.GetRolesAsync(user)).ToList();
    }

    public async Task DeleteUserAsync(string identityUserId)
    {
        var user = await userManager.FindByIdAsync(identityUserId)
            ?? throw new InvalidOperationException("User not found.");

        await userManager.SetLockoutEnabledAsync(user, true);
        var lockoutResult = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        if (!lockoutResult.Succeeded)
        {
            throw BuildIdentityOperationException(lockoutResult, "Failed to delete user.");
        }

        await userManager.UpdateSecurityStampAsync(user);

        using var dbContext = dbContextFactory.CreateDbContext();
        var shelfUser = await dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
        if (shelfUser is not null)
        {
            shelfUser.IsDeleted = true;
            shelfUser.DeletedAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task RestoreUserAsync(string identityUserId)
    {
        var user = await userManager.FindByIdAsync(identityUserId)
            ?? throw new InvalidOperationException("User not found.");

        await userManager.SetLockoutEnabledAsync(user, true);
        var unlockResult = await userManager.SetLockoutEndDateAsync(user, null);
        if (!unlockResult.Succeeded)
        {
            throw BuildIdentityOperationException(unlockResult, "Failed to restore user.");
        }

        await userManager.UpdateSecurityStampAsync(user);

        using var dbContext = dbContextFactory.CreateDbContext();
        var shelfUser = await dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);

        if (shelfUser is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        shelfUser.IsDeleted = false;
        shelfUser.DeletedAt = null;
        await dbContext.SaveChangesAsync();
    }

    private static bool IsKnownRole(string role)
    {
        return KnownRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    private static InvalidOperationException BuildIdentityOperationException(IdentityResult result, string fallbackMessage)
    {
        var errorMessage = result.Errors.Any()
            ? string.Join("; ", result.Errors.Select(x => x.Description))
            : fallbackMessage;

        return new InvalidOperationException(errorMessage);
    }
}