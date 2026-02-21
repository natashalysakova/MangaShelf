using MangaShelf.Common.Localization.Services;
using MangaShelf.Localization.Interfaces;
using MangaShelf.Localization.Resources;
using MangaShelf.Localization.Resources.Components;
using MangaShelf.Localization.Resources.Elements;
using Microsoft.Extensions.Localization;

namespace MangaShelf.Localization.Services;

internal class UserInterfaceLocalizationService : LocalizationService<UserInterfaceResource>, IUiLocalizationService
{
    public UserInterfaceLocalizationService(IStringLocalizer<UserInterfaceResource> localizer, ILogger<UserInterfaceLocalizationService> logger)
        : base(localizer, logger)
    {
    }
}


internal class VolumePageLocalizationService : LocalizationService<VolumePageResource>, IVolumePageLocalizationService
{
    public VolumePageLocalizationService(IStringLocalizer<VolumePageResource> localizer, ILogger<VolumePageLocalizationService> logger)
        : base(localizer, logger)
    {
    }
}

internal class AdminLocalizationService : LocalizationService<AdminInterfaceResource>, IAdminLocalizationService
{
    public AdminLocalizationService(IStringLocalizer<AdminInterfaceResource> localizer, ILogger<AdminLocalizationService> logger)
        : base(localizer, logger)
    {
    }
}

internal class SmallCardLocalizationService : LocalizationService<SmallCardResource>, ISmallCardLocalizationService
{
    public SmallCardLocalizationService(IStringLocalizer<SmallCardResource> localizer, ILogger<SmallCardLocalizationService> logger)
        : base(localizer, logger)
    {
    }
}