using MangaShelf.BL.Interfaces;
using MangaShelf.BL.Services;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.MangaShelf;
using MangaShelf.Data;
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

    public static class IdentityContextInstallExtention
    {
        public static IHostApplicationBuilder RegisterIdentityContextAndServices(this IHostApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("AccountsDb") ?? throw new InvalidOperationException("Connection string 'AccountsDb' not found.");

            var accontDbVersion = ServerVersion.AutoDetect(connectionString);
            builder.Services.AddDbContext<ApplicationDbContext>(
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


            builder.Services.AddIdentityCore<ApplicationUser>(options =>
             {
                 options.SignIn.RequireConfirmedAccount = true;
             })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
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
            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

            return builder;
        }

        public static async Task MakeSureAccountDbCreatedAsync(this IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await dbContext.Database.MigrateAsync();
            }
        }
    }
}
