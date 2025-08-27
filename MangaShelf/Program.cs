using MangaShelf.BL.Interfaces;
using MangaShelf.BL.Services;
using MangaShelf.Common.Localization.Services;
using MangaShelf.Components;
using MangaShelf.Components.Account;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Repositories;
using MangaShelf.Extentions;
using MangaShelf.Infrastructure.Accounts;
using MangaShelf.Infrastructure.Installer;
using MangaShelf.Localization.Interfaces;
using MangaShelf.Localization.Services;
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
        builder.Services.AddControllers();

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.RegisterContextAndServices();
        builder.RegisterIdentityContextAndServices();
        builder.Services.AddScoped<IdentityUserAccessor>();
        builder.Services.AddScoped<IdentityRedirectManager>();

        builder.Services.AddHealthChecks();

        builder.AddBusinessServices();

        builder.Services.AddLocalization();
        builder.AddLocalizationServices();
        builder.AddUILocalizationServices();

        builder.Services.AddMudServices();

        var app = builder.Build();

        await app.Services.MakeSureDbCreatedAsync();

        app.MapHealthChecks("/health");

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.UseStaticFiles();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        // Add additional endpoints required by the Identity /Account Razor components.
        app.MapAdditionalIdentityEndpoints();

        string[] supportedCultures = LocalizationService.SupportedCultures.Select(x=>x.Name).ToArray();
        var localizationOptions = new RequestLocalizationOptions()
            .SetDefaultCulture(supportedCultures[0])
            .AddSupportedCultures(supportedCultures)
            .AddSupportedUICultures(supportedCultures);
        app.UseRequestLocalization(localizationOptions);

        app.MapControllers();

        app.Run();
    }
}
