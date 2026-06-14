using MangaShelf.Common.Localization.Services;
using MangaShelf.Localization.Interfaces;
using MangaShelf.Localization.Resources;
using Microsoft.Extensions.Localization;

namespace MangaShelf.Localization.Services;

internal class UserInterfaceLocalizationService : LocalizationService<UserInterfaceResource>, IUiLocalizationService
{
    public UserInterfaceLocalizationService(IStringLocalizer<UserInterfaceResource> localizer, ILogger<UserInterfaceLocalizationService> logger)
        : base(localizer, logger)
    {
    }
}
