using MangaShelf.BL.Interfaces;
using MangaShelf.BL.Services;
using MangaShelf.DAL;
using MangaShelf.DAL.Interceptors;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using MangaShelf.DAL.Models;
using MangaShelf.Infrastructure.Accounts;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace MangaShelf.Infrastructure.Installer
{
    public static class ServicesInstallExtention
    {
        public static IHostApplicationBuilder RegisterServices(this IHostApplicationBuilder builder)
        {
            builder.Services.AddScoped<ICountryService, CountryService>();
            builder.Services.AddScoped<ICountryRepository, CountryRepository>();


            return builder;
        }
    }

    public static class ContextInstallExtention
    {
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

            builder.Services.AddIdentityCore<User>(options =>
             {
                 options.SignIn.RequireConfirmedAccount = true;
             })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<MangaDbContext>()
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
            builder.Services.AddSingleton<IEmailSender<User>, IdentityNoOpEmailSender>();

            return builder;
        }

        public static async Task MakeSureDbCreatedAsync(this IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MangaDbContext>();
                await dbContext.Database.MigrateAsync();
            }
        }
    }
}
