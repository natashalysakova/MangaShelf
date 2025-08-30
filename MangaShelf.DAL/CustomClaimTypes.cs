using System.Security.Claims;

namespace MangaShelf.DAL;

public static class CustomClaimTypes
{

    /// <summary>
    /// public async Task InvokeAsync (HttpContext context)
    // {
    //     if (context.User.Identity.IsAuthenticated)
    //     {
    //         if (context.User.HasClaim("MUST_CHANGE_PASSWORD","true") && context.Request.Path != "/Account/ChangePassword")
    //         {
    //             context.Response.Redirect("/Account/ChangePassword");
    //             return;
    //         }
    //     }

    //     await _next(context);
    // }
    /// </summary>

    public const string MustChangePassword = "MUST_CHANGE_PASSWORD";
    public const string CannotChangePassword = "CANNOT_CHANGE_PASSWORD";
    public const string IsDemoUser = "IS_DEMO_USER";
}

public static class CustomClaimTypesExtensions
{
    public static bool MustChangePassword(this ClaimsPrincipal user)
        => user.HasClaim(CustomClaimTypes.MustChangePassword, "true");
    public static bool CannotChangePassword(this ClaimsPrincipal user)
        => user.HasClaim(CustomClaimTypes.CannotChangePassword, "true");
    public static bool IsDemoUser(this ClaimsPrincipal user)
        => user.HasClaim(CustomClaimTypes.IsDemoUser, "true");
}
