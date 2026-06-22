using MangaShelf.BL.Contracts;
using MangaShelf.DAL;
using MangaShelf.DAL.Identity;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.BL.Services;

public class UserService : IUserService
{
    private readonly IDbContextFactory<MangaDbContext> dbContextFactory;

    public UserService(IDbContextFactory<MangaDbContext> dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
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
}