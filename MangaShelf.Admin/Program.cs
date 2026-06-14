using MangaShelf.Admin.Components;
using MangaShelf.Admin.Localization.Interfaces;
using MangaShelf.Infrastructure.Accounts;
using MangaShelf.Infrastructure.Installer;
using Microsoft.AspNetCore.Authentication.Cookies;
using MudBlazor.Services;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

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

        //builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.RegisterContextAndServices();
        builder.RegisterIdentityContextAndServices();
        //builder.Services.AddScoped<IdentityUserAccessor>();
        //builder.Services.AddScoped<IdentityRedirectManager>();

        builder.Services.AddHealthChecks();

        builder.AddBusinessServices();

        builder.Services.AddLocalization();
        builder.AddLocalizationServices();
        builder.AddAdminLocalizationServices();

        builder.Services.AddHttpContextAccessor();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();

        app.UseRequestLocalization();
        app.UseStatusCodePagesWithRedirects("/404");

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();


        app.UseStaticFiles();
        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        // Add additional endpoints required by the Identity /Account Razor components.
        app.MapAdditionalIdentityEndpoints();

        app.MapControllers();

        app.Run();
    }
}

public static class AdminUiExtension
{
    public static void AddAdminLocalizationServices(this IHostApplicationBuilder builder)
    {
        var assembly = typeof(IAdminLocalizationService).Assembly;
        builder.Services.AddLocalizationServicesFromAssembly(assembly);
    }
}

