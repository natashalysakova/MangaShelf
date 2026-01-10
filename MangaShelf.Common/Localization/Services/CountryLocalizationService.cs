using MangaShelf.Common.Localization.Interfaces;
using MangaShelf.Common.Localization.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace MangaShelf.Common.Localization.Services;

public class CountryLocalizationService : LocalizationService<CountryResource>, ICountryLocalizationService
{
    public CountryLocalizationService(IStringLocalizer<CountryResource> localizer, ILogger<CountryLocalizationService> logger)
        : base(localizer, logger)
    {
    }
}
