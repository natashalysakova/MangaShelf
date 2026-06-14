using MangaShelf.Common.Localization.Services;
using MangaShelf.Localization.Interfaces;
using MangaShelf.Localization.Resources.Components;
using Microsoft.Extensions.Localization;

namespace MangaShelf.Localization.Services;

internal class VolumePageLocalizationService : LocalizationService<VolumePageResource>, IVolumePageLocalizationService
{
    public VolumePageLocalizationService(IStringLocalizer<VolumePageResource> localizer, ILogger<VolumePageLocalizationService> logger)
        : base(localizer, logger)
    {
    }
}
