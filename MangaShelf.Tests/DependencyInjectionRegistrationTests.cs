using MangaShelf.Cache;
using MangaShelf.Components.Account;
using MangaShelf.Extentions;
using MangaShelf.Infrastructure.Installer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MangaShelf.Tests;

public class DependencyInjectionRegistrationTests
{
    private static readonly string[] ApplicationAssemblyPrefixes = ["MangaShelf"];

    [Fact]
    public void All_Registered_Services_Can_Be_Resolved()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:MangaDb"] = "Server=localhost;Database=MangaDb;User=test;Password=test;",
            ["ConnectionStrings:SystemDb"] = "Server=localhost;Database=SystemDb;User=test;Password=test;",
            ["ConnectionStrings:AccountsDb"] = "Server=localhost;Database=AccountsDb;User=test;Password=test;"
        });

        builder.Services.AddLogging();
        builder.Services.AddAuthorization();
        builder.Services.AddControllers();
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddLocalization();

        builder.RegisterContextAndServices();
        builder.RegisterIdentityContextAndServices();
        builder.AddBusinessServices();
        builder.RegisterCacheServices();
        builder.AddLocalizationServices();
        builder.AddUILocalizationServices();
        builder.AddUiStateServices();

        builder.Services.AddScoped<IdentityUserAccessor>();
        builder.Services.AddScoped<IdentityRedirectManager>();

        var descriptorsToValidate = builder.Services
            .Where(static descriptor =>
                descriptor.ImplementationInstance is null &&
                IsApplicationService(descriptor))
            .ToList();

        using var serviceProvider = builder.Services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var failures = new List<string>();

        foreach (var descriptor in descriptorsToValidate)
        {
            try
            {
                if (descriptor.ServiceKey is null)
                {
                    _ = scope.ServiceProvider.GetRequiredService(descriptor.ServiceType);
                }
                else
                {
                    _ = scope.ServiceProvider.GetRequiredKeyedService(descriptor.ServiceType, descriptor.ServiceKey);
                }
            }
            catch (Exception ex)
            {
                failures.Add($"{FormatDescriptor(descriptor)} => {ex.GetBaseException().Message}");
            }
        }

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    private static bool IsApplicationService(ServiceDescriptor descriptor)
    {
        return IsApplicationType(descriptor.ServiceType)
            || IsApplicationType(descriptor.ImplementationType)
            || IsApplicationType(descriptor.ImplementationFactory?.Method.ReturnType);
    }

    private static bool IsApplicationType(Type? type)
    {
        if (type is null)
        {
            return false;
        }

        var assemblyName = type.Assembly.GetName().Name;
        return assemblyName is not null && ApplicationAssemblyPrefixes.Any(assemblyName.StartsWith);
    }

    private static string FormatDescriptor(ServiceDescriptor descriptor)
    {
        var implementation = descriptor.ImplementationType?.FullName
            ?? descriptor.ImplementationInstance?.GetType().FullName
            ?? descriptor.ImplementationFactory?.Method.ReturnType.FullName
            ?? "factory";

        return descriptor.ServiceKey is null
            ? $"{descriptor.Lifetime} {descriptor.ServiceType.FullName} -> {implementation}"
            : $"{descriptor.Lifetime} {descriptor.ServiceType.FullName} [{descriptor.ServiceKey}] -> {implementation}";
    }
}
