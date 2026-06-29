using MangaShelf.DAL.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MangaShelf.BL.Contracts;
using MangaShelf.Localization.Interfaces;
using MangaShelf.Controllers.Models;

namespace MangaShelf.Controllers;

[Route("[controller]/[action]")]
public class AuthController : Controller
{
    private readonly IUserStore<MangaIdentityUser> userStore;
    private readonly UserManager<MangaIdentityUser> userManager;
    private readonly SignInManager<MangaIdentityUser> signInManager;
    private readonly IUserService userService;
    private readonly ILogger<AuthController> logger;
    private readonly IUiLocalizationService localizer;

    public AuthController(
        IUserStore<MangaIdentityUser> userStore,
        UserManager<MangaIdentityUser> userManager,
        SignInManager<MangaIdentityUser> signInManager,
        IUserService userService,
        ILogger<AuthController> logger,
        IUiLocalizationService localizer)
    {
        this.userStore = userStore;
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.userService = userService;
        this.logger = logger;
        this.localizer = localizer;
    }
    [HttpPost]
    public async Task<IActionResult> Login(string username, string password, bool rememberMe, string? returnUrl = null)
    {
        using var scope = HttpContext.RequestServices.CreateScope();
        var _signInManager =
            scope.ServiceProvider.GetRequiredService<SignInManager<MangaIdentityUser>>();

        var result = await _signInManager.PasswordSignInAsync(
            username, password,
            isPersistent: rememberMe, lockoutOnFailure: false);


        if (result.Succeeded)
        {
            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
            {
                returnUrl = "/";
            }

            // Redirect to home page with a full reload to refresh the Blazor circuit
            return Redirect(returnUrl);
        }
        // Redirect back to login with error

        var errorMessage = localizer["Invalid login attempt"];
        return Redirect($"/Account/Login?error={Uri.EscapeDataString(errorMessage)}");
    }

    [HttpPost]
    public async Task<IActionResult> Logout(string? returnUrl = null)
    {
        using var scope = HttpContext.RequestServices.CreateScope();
        var _signInManager =
            scope.ServiceProvider.GetRequiredService<SignInManager<MangaIdentityUser>>();

        await _signInManager.SignOutAsync();

        // Redirect to home page with a full reload to refresh the Blazor circuit
        return RedirectToUrl(returnUrl);
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromForm] RegisterInputModel model, string? returnUrl = null)
    {
        var user = CreateUser();
        await userStore.SetUserNameAsync(user, model.Username, CancellationToken.None);
        var emailStore = GetEmailStore();
        await emailStore.SetEmailAsync(user, model.Email, CancellationToken.None);
        var result = await userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            var errorMessage = string.Join(' ', result.Errors.Select(e => e.Description));
            return Redirect($"/Account/Register?error={Uri.EscapeDataString(errorMessage)}&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
        }

        logger.LogInformation("User created a new account with password.");

        var userId = await userManager.GetUserIdAsync(user);

        await userManager.AddToRoleAsync(user, "User");

        var shelfUser = await userService.RegisterShelfUserAsync(user);


        await signInManager.SignInAsync(user, isPersistent: true);
        if (!result.Succeeded)
        {
            var errorMessage = localizer["Invalid registration attempt"];
            return Redirect($"/Account/Register?error={Uri.EscapeDataString(errorMessage)}&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
        }

        return RedirectToUrl(returnUrl);

    }

    private IActionResult RedirectToUrl(string? returnUrl)
    {
        if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
        {
            returnUrl = "/";
        }

        return Redirect(returnUrl);
    }

    private MangaIdentityUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<MangaIdentityUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(User)}'. " +
                $"Ensure that '{nameof(User)}' is not an abstract class and has a parameterless constructor.");
        }
    }

    private IUserEmailStore<MangaIdentityUser> GetEmailStore()
    {
        if (!userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<MangaIdentityUser>)userStore;
    }
}