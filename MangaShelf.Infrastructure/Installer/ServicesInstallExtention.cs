using MangaShelf.BL.Interfaces;
using MangaShelf.BL.Services;
using MangaShelf.DAL;
using MangaShelf.DAL.Identity;
using MangaShelf.DAL.Interceptors;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using MangaShelf.DAL.Repositories;
using MangaShelf.Infrastructure.Accounts;
using MangaShelf.SeedService;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;


namespace MangaShelf.Infrastructure.Installer;

public static class ServicesInstallExtention
{
    public static IHostApplicationBuilder RegisterServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<ICountryService, CountryService>();
        builder.Services.AddScoped<ICountryRepository, CountryRepository>();
        builder.Services.AddScoped<IVolumeService, VolumeService>();
        builder.Services.AddScoped<IVolumeRepository, VolumeRepository>();
        builder.Services.AddScoped<ISeriesService, SeriesService>();
        builder.Services.AddScoped<ISeriesRepository, SeriesRepository>();
        builder.Services.AddScoped<IAuthorService, AuthorService>();
        builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
        builder.Services.AddScoped<IPublisherService, PublisherService>();
        builder.Services.AddScoped<IPublisherRepository, PublisherRepository>();

        builder.RegisterSeedServices();
        return builder;
    }
}

public static class ContextInstallExtention
{
    public static IHostApplicationBuilder RegisterIdentityContextAndServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("AccountsDb") ?? throw new InvalidOperationException("Connection string 'AccountsDb' not found.");

        var accontDbVersion = ServerVersion.AutoDetect(connectionString);
        builder.Services.AddDbContext<MangaIdentityDbContext>(
            options =>
            {
                options
                .UseMySql(connectionString, accontDbVersion,
                    mysqlOptions =>
                    {
                        mysqlOptions.EnableRetryOnFailure();
                    });

                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                }
            });

        builder.Services.AddIdentityCore<MangaIdentityUser>(options =>
         {
             options.SignIn.RequireConfirmedAccount = true;
         })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<MangaIdentityDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
        .AddIdentityCookies();

        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
        builder.Services.AddSingleton<IEmailSender<MangaIdentityUser>, IdentityNoOpEmailSender>();

        return builder;
    }

    public static IHostApplicationBuilder RegisterContextAndServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("MangaDb") ?? throw new InvalidOperationException("Connection string 'MangaDb' not found.");

        var accontDbVersion = ServerVersion.AutoDetect(connectionString);
        builder.Services.AddDbContext<MangaDbContext>(
            options =>
            {
                options
                .UseMySql(connectionString, accontDbVersion,
                mysqlOptions =>
                {
                    mysqlOptions.EnableRetryOnFailure();
                })
                .AddInterceptors(new AuditInterceptor());

                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                }
            });

        return builder;
    }

    public static async Task MakeSureDbCreatedAsync(this IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<MangaDbContext>();
            await dbContext.Database.MigrateAsync();

            var IdentityDbContext = scope.ServiceProvider.GetRequiredService<MangaIdentityDbContext>();
            await IdentityDbContext.Database.MigrateAsync();

            await SeedDatabase(scope);
        }
    }

    private static async Task SeedDatabase(IServiceScope scope)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IServiceProvider>>();
        logger.LogInformation("Seeding Started");
        foreach (var service in scope.ServiceProvider.GetServices<ISeedDataService>().OrderBy(x => x.Priority))
        {
            try
            {
                logger.LogInformation("{Activity} started", service.ActivitySourceName);
                await service.Run(scope.ServiceProvider);
                logger.LogInformation("{Activity} finished", service.ActivitySourceName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during seeding: {ErrorMessage}", ex.Message);
            }
        }
        logger.LogInformation("Seeding done");
    }
}
