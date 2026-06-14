using MangaShelf.Common.Localization.Services;
using MangaShelf.Localization.Interfaces;
using MangaShelf.Localization.Resources.Elements;
using Microsoft.Extensions.Localization;

namespace MangaShelf.Localization.Services;

internal class SmallCardLocalizationService : LocalizationService<SmallCardResource>, ISmallCardLocalizationService
{
    public SmallCardLocalizationService(IStringLocalizer<SmallCardResource> localizer, ILogger<SmallCardLocalizationService> logger)
        : base(localizer, logger)
    {
    }
}