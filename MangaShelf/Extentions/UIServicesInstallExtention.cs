using MangaShelf.Localization.Interfaces;
using MangaShelf.Localization.Services;

namespace MangaShelf.Extentions;

public static class UIServicesInstallExtention
{
    public static void AddUILocalizationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IUiLocalizationService, UserInterfaceLocalizationService>();
    }
}