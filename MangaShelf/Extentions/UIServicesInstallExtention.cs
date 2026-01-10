using MangaShelf.Common.Localization.Interfaces;
using MangaShelf.Localization.Interfaces;
using MangaShelf.Localization.Services;
using MangaShelf.Services;
using System.Reflection;

namespace MangaShelf.Extentions;

public static class UIServicesInstallExtention
{
    public static void AddUILocalizationServices(this IHostApplicationBuilder builder)
    {
        var assembly = typeof(UserInterfaceLocalizationService).Assembly;
        builder.Services.AddLocalizationServicesFromAssembly(assembly);
    }

    private static IServiceCollection AddLocalizationServicesFromAssembly(
        this IServiceCollection services,
        Assembly assembly)
    {
        var localizationServiceType = typeof(ILocalizationService<>);

        var serviceTypes = assembly.GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } &&
                type.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == localizationServiceType))
            .ToList();

        foreach (var implementationType in serviceTypes)
        {
            // Find the most specific interface (non-generic preferred)
            var interfaceType = implementationType.GetInterfaces()
                .Where(i => i != typeof(IAutoRegisterLocalizationService) &&
                           (i.IsGenericType && i.GetGenericTypeDefinition() == localizationServiceType ||
                            i.GetInterfaces().Any(ii => ii.IsGenericType && ii.GetGenericTypeDefinition() == localizationServiceType)))
                .OrderBy(i => i.IsGenericType ? 1 : 0) // Prefer non-generic interfaces
                .FirstOrDefault();

            if (interfaceType != null)
            {
                services.AddSingleton(interfaceType, implementationType);
            }
        }

        return services;
    }

    public static void AddUiStateServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IVolumeStateService, VolumeStateService>();
    }
}