using MangaShelf.Cache;
using MangaShelf.Common.Localization.Services;
using MangaShelf.Components;
using MangaShelf.Components.Account;
using MangaShelf.Extentions;
using MangaShelf.Infrastructure.Accounts;
using MangaShelf.Infrastructure.Installer;
using MangaShelf.Infrastructure.Network;
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
                options.Cookie.MaxAge = TimeSpan.FromHours(24);
            });
        
        builder.Services.AddControllers();
        builder.Services.AddMudServices();

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.RegisterContextAndServices();
        builder.RegisterIdentityContextAndServices();
        builder.Services.AddScoped<IdentityUserAccessor>();
        builder.Services.AddScoped<IdentityRedirectManager>();

        builder.Services.AddHealthChecks();

        builder.AddBusinessServices();

        builder.Services
            .Configure<CacheOptions>(
                builder.Configuration
                .GetSection(CacheOptions.SectionName));
        builder.Services
            .Configure<HtmlDownloadOptions>(
                builder.Configuration
                .GetSection(HtmlDownloadOptions.SectionName));

        builder.Services.AddHostedService<CacheWorker>();
        builder.Services.AddSingleton<CacheSignal>();

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

        await app.MakeSureDbCreatedAsync();

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

        app.Run();
    }
}
