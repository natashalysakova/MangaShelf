using MangaShelf.DAL;
using MangaShelf.DAL.Identity;
using MangaShelf.DAL.Interceptors;
using MangaShelf.DAL.System;
using MangaShelf.DAL.System.Models;
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

    public static async Task MakeSureDbCreatedAsync(this IHost host)
    {
        using (var scope = host.Services.CreateScope())
        {
            await MakeSureDbCreatedAsync<MangaSystemDbContext>(scope);
            await MakeSureDbCreatedAsync<MangaDbContext>(scope);
            await MakeSureDbCreatedAsync<MangaIdentityDbContext>(scope);

            //var systemContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MangaSystemDbContext>>();
            //var SystemDbContext = systemContextFactory.CreateDbContext();
            //var str = SystemDbContext.Database.GetConnectionString();

            //if (SystemDbContext.Database.HasPendingModelChanges())
            //{
            //    throw new InvalidOperationException("System database model has pending changes. Please apply migrations before starting the application.");
            //}

            //await SystemDbContext.Database.MigrateAsync();

            //await ResetStuckJobs(SystemDbContext);

            //var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MangaDbContext>>();
            //var context = await factory.CreateDbContextAsync();
            //if (context.Database.HasPendingModelChanges())
            //{
            //    throw new InvalidOperationException("System database model has pending changes. Please apply migrations before starting the application.");
            //}

            //await context.Database.MigrateAsync();

            //var IdentityDbContext = scope.ServiceProvider.GetRequiredService<MangaIdentityDbContext>();
            //await IdentityDbContext.Database.MigrateAsync();
        }
    }

    public static async Task MakeSureDbCreatedAsync<T>(IServiceScope scope) where T : DbContext
    {
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<T>>();
        var context = await factory.CreateDbContextAsync();
        if (context.Database.HasPendingModelChanges())
        {
            throw new InvalidOperationException($"System database model ({typeof(T).Name}) has pending changes. Please apply migrations before starting the application.");
        }

        await context.Database.MigrateAsync();

        var IdentityDbContext = scope.ServiceProvider.GetRequiredService<T>();
        await IdentityDbContext.Database.MigrateAsync();
    }

    public static async Task SeedDatabase(this IHost host)
    {
        using var scope = host.Services.CreateScope();
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

    private static async Task ResetStuckJobs(MangaSystemDbContext context)
    {
        try
        {
            var parserStatuses = context.Parsers
                .Include(p => p.Jobs)
                    .ThenInclude(r => r.Errors);

            var notFinishedProperly = parserStatuses
                .SelectMany(x => x.Jobs)
                .Where(r => r.Status == RunStatus.Waiting || r.Status == RunStatus.Running);

            foreach (var job in notFinishedProperly)
            {
                job.Status = RunStatus.Error;
                job.Finished = DateTimeOffset.Now;
                job.Progress = -1;
                job.Errors.Add(new ParserError()
                {
                    ErrorMessage = "Was automatically cancelled after restart",
                    RunTime = job.Finished.Value
                });
            }

            foreach (var parser in parserStatuses)
            {
                parser.Status = ParserStatus.Idle;
            }

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // do nothing, we don't want to block the app from starting
        }
    }
}
