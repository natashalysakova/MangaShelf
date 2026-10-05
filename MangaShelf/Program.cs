using MangaShelf.Cache;
using MangaShelf.Common.Localization.Services;
using MangaShelf.Components;
using MangaShelf.Components.Account;
using MangaShelf.DAL.Identity;
using MangaShelf.Extentions;
using MangaShelf.Infrastructure.Accounts;
using MangaShelf.Infrastructure.Installer;
using MangaShelf.Localization.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using MudBlazor.Services;

namespace MangaShelf;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        //Authentication
        builder.Services.AddAuthorization();

        //The cookie authentication is never used, but it is required to prevent a runtime error
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "auth_cookie";
                options.Cookie.MaxAge = TimeSpan.FromDays(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                if (builder.Environment.IsDevelopment())
                {
                    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.None;
                }
            });
        
        builder.Services.AddControllers().AddDataAnnotationsLocalization(options =>
        {
            options.DataAnnotationLocalizerProvider = (type, factory) =>
                factory.Create(typeof(UserInterfaceLocalizationService));
        });
        builder.Services.AddMudServices();

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.RegisterContextAndServices();
        builder.RegisterIdentityContextAndServices();
        builder.Services.AddScoped<IdentityUserAccessor>();
        builder.Services.AddScoped<IdentityRedirectManager>();

        builder.Services.AddHealthChecks();

        builder.AddBusinessServices();
        builder.RegisterCacheServices();

        builder.Services.AddLocalization();
        builder.AddLocalizationServices();
        builder.AddUILocalizationServices();

        builder.AddUiStateServices();

        builder.Services.AddHttpContextAccessor();


        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        }

        builder.Services.AddHttpClient("LocalApi", client => client.BaseAddress = new Uri("http://localhost:5090/"));

        var app = builder.Build();

        app.MapHealthChecks("/health");

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
            app.UseExceptionHandler("/Error");

        }
        else
        {
            app.UseExceptionHandler("/Error");
            // HSTS disabled - HTTPS not required
            // app.UseHsts();
        }

        // app.UseHttpsRedirection(); // Disabled - HTTPS not required

        app.UseStaticFiles();
        app.MapStaticAssets();
        
        string[] supportedCultures = LocalizationService.SupportedCultures.Select(x => x.Name).ToArray();
        var localizationOptions = new RequestLocalizationOptions()
            .SetDefaultCulture(supportedCultures[0])
            .AddSupportedCultures(supportedCultures)
            .AddSupportedUICultures(supportedCultures);
        localizationOptions.ApplyCurrentCultureToResponseHeaders = true;
        app.UseRequestLocalization(localizationOptions);

        app.UseStatusCodePagesWithRedirects("/404");

        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        // Add additional endpoints required by the Identity /Account Razor components.
        app.MapAdditionalIdentityEndpoints();

        app.MapControllers();

        if (app.Environment.IsDevelopment())
        {
            app.MapGet("/dev-login", async (
                HttpContext context,
                UserManager<MangaIdentityUser> userManager,
                SignInManager<MangaIdentityUser> signInManager,
                IConfiguration configuration) =>
            {
                var username = configuration["DevLogin:Username"];
                if (string.IsNullOrWhiteSpace(username))
                {
                    return Results.Problem("Set DevLogin:Username in user secrets.");
                }

                var user = await userManager.FindByNameAsync(username);
                if (user is null)
                {
                    return Results.NotFound("Development login user was not found.");
                }

                await signInManager.SignInAsync(user, isPersistent: false);
                return Results.LocalRedirect("/");
            }).AllowAnonymous();
        }

        app.Run();
    }
}
