using MangaShelf.DAL.Identity;
using Microsoft.AspNetCore.Identity;

namespace MangaShelf.Components.Account;

public sealed class IdentityUserAccessor(UserManager<MangaIdentityUser> userManager, IdentityRedirectManager redirectManager)
{
    public async Task<MangaIdentityUser> GetRequiredUserAsync(HttpContext context)
    {
        var user = await userManager.GetUserAsync(context.User);

        if (user is null)
        {
            redirectManager.RedirectToWithStatus("Account/InvalidUser", $"Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.", context);
        }

        return user;
    }
}
