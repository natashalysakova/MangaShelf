using MangaShelf.DAL;
using MangaShelf.DAL.Identity;
using MangaShelf.DAL.Interceptors;
using MangaShelf.DAL.System;
using MangaShelf.DAL.System.Models;
using MangaShelf.Infrastructure.Accounts;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


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
        RegisterSystemContextAndServices(builder);
        RegisterShelfContextAndServices(builder);

        return builder;
    }

    private static IHostApplicationBuilder RegisterShelfContextAndServices(IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("MangaDb") ?? throw new InvalidOperationException("Connection string 'MangaDb' not found.");

        var accontDbVersion = ServerVersion.AutoDetect(connectionString);
        var sqlConfiguration = new Action<DbContextOptionsBuilder>(options =>
        {
            if (builder.Environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
            }

            options
            .UseMySql(connectionString, accontDbVersion,
            mysqlOptions =>
            {
                mysqlOptions.EnableRetryOnFailure();
            })
            .AddInterceptors(new AuditInterceptor());
        });


        builder.Services.AddDbContext<MangaDbContext>(options => sqlConfiguration(options), optionsLifetime: ServiceLifetime.Singleton);
        builder.Services.AddDbContextFactory<MangaDbContext>(options => sqlConfiguration(options));

        return builder;
    }

    private static IHostApplicationBuilder RegisterSystemContextAndServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("SystemDb") ?? throw new InvalidOperationException("Connection string 'SystemDb' not found.");

        var accontDbVersion = ServerVersion.AutoDetect(connectionString);
        var sqlConfiguration = new Action<DbContextOptionsBuilder>(options =>
        {
            if (builder.Environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
            }

            options
            .UseMySql(connectionString, accontDbVersion,
            mysqlOptions =>
            {
                mysqlOptions.EnableRetryOnFailure();
            });
        });


        builder.Services.AddDbContext<MangaSystemDbContext>(options => sqlConfiguration(options), optionsLifetime: ServiceLifetime.Singleton);
        builder.Services.AddDbContextFactory<MangaSystemDbContext>(options => sqlConfiguration(options));

        return builder;
    }



    

    
}
