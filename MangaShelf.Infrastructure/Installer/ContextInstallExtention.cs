using MangaShelf.DAL;
using MangaShelf.DAL.Identity;
using MangaShelf.DAL.Interceptors;
using MangaShelf.Infrastructure.Accounts;
using MangaShelf.Infrastructure.Seed;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace MangaShelf.Infrastructure.Installer;

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
        }
    }

    public static async Task SeedDatabase(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IServiceProvider>>();
        logger.LogInformation("Seeding Started");
        foreach (var service in scope.ServiceProvider.GetServices<ISeedDataService>().OrderBy(x => x.Priority))
        {
            try
            {
                logger.LogInformation("{Activity} started", service.ActivitySourceName);
                await service.Run();
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
