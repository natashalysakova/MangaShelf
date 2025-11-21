using MangaShelf.DAL.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MangaShelf.Controllers;

[Route("[controller]/[action]")]
public class AuthController : Controller
{
    IServiceProvider _serviceProvider;
    public AuthController(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    [HttpPost]
    public async Task<IActionResult> Login(string username, string password, bool rememberMe = false)
    {
        using var scope = HttpContext.RequestServices.CreateScope();
        var _signInManager = 
            scope.ServiceProvider.GetRequiredService<SignInManager<MangaIdentityUser>>();

        var result = await _signInManager.PasswordSignInAsync(
            username, password, 
            isPersistent: rememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            // Redirect to home page with a full reload to refresh the Blazor circuit
            return Redirect("/");
        }
        // Redirect back to login with error
        return Redirect("/Account/Login?error=Invalid login attempt");
    }
}