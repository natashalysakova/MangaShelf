using MangaShelf.Cache;
using MangaShelf.Common.Localization.Services;
using MangaShelf.Components;
using MangaShelf.Components.Account;
using MangaShelf.Extentions;
using MangaShelf.Infrastructure.Accounts;
using MangaShelf.Infrastructure.Installer;
using MangaShelf.Infrastructure.Network;
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

        builder.Services.AddHttpContextAccessor();

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        }

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
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();


        app.UseStaticFiles();
        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        // Add additional endpoints required by the Identity /Account Razor components.
        app.MapAdditionalIdentityEndpoints();

        string[] supportedCultures = LocalizationService.SupportedCultures.Select(x => x.Name).ToArray();
        var localizationOptions = new RequestLocalizationOptions()
            .SetDefaultCulture(supportedCultures[0])
            .AddSupportedCultures(supportedCultures)
            .AddSupportedUICultures(supportedCultures);
        localizationOptions.ApplyCurrentCultureToResponseHeaders = true;
        app.UseRequestLocalization(localizationOptions);

        app.UseStatusCodePagesWithRedirects("/404");


        app.UseAuthentication();
        app.UseRouting();

        app.UseAntiforgery();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
