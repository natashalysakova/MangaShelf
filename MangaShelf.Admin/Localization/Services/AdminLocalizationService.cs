using MangaShelf.Admin.Localization.Interfaces;
using MangaShelf.Admin.Localization.Resources;
using MangaShelf.Common.Localization.Services;

using Microsoft.Extensions.Localization;

namespace MangaShelf.Admin.Localization.Services;


internal class AdminLocalizationService : LocalizationService<AdminInterfaceResource>, IAdminLocalizationService
{
    public AdminLocalizationService(IStringLocalizer<AdminInterfaceResource> localizer, ILogger<AdminLocalizationService> logger)
        : base(localizer, logger)
    {
    }
}