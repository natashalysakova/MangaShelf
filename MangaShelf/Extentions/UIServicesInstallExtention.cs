using MangaShelf.Common.Localization.Interfaces;
using MangaShelf.Infrastructure.Installer;
using MangaShelf.Localization.Services;
using MangaShelf.Services;

namespace MangaShelf.Extentions;

public static class UIServicesInstallExtention
{
    public static void AddUILocalizationServices(this IHostApplicationBuilder builder)
    {
        var assembly = typeof(UserInterfaceLocalizationService).Assembly;
        builder.Services.AddLocalizationServicesFromAssembly(assembly);
    }

    public static void AddUiStateServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IVolumeStateService, VolumeStateService>();
    }
}