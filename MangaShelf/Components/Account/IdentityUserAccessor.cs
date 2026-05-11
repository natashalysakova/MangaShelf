using MangaShelf.DAL.Identity;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace MangaShelf.Components.Account;

public sealed class IdentityUserAccessor(UserManager<MangaIdentityUser> userManager, IdentityRedirectManager redirectManager)
{
    public async Task<MangaIdentityUser> GetRequiredUserAsync(ClaimsPrincipal principal)
    {
        var user = await userManager.GetUserAsync(principal);

        if (user is null)
        {
            redirectManager.RedirectToWithStatus("Account/InvalidUser", $"Error: Unable to load user with ID '{userManager.GetUserId(principal)}'.");
        }

        return user;
    }
}
