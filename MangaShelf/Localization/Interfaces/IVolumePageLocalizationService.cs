using MangaShelf.Common.Localization.Interfaces;
using MangaShelf.Localization.Resources;
using MangaShelf.Localization.Resources.Components;
using MangaShelf.Localization.Resources.Elements;

namespace MangaShelf.Localization.Interfaces;

public interface IVolumePageLocalizationService : ILocalizationService<VolumePageResource>
{
}

public interface ISmallCardLocalizationService : ILocalizationService<SmallCardResource>
{
}

public interface IUiLocalizationService : ILocalizationService<UserInterfaceResource>
{
}

public interface IAdminLocalizationService : ILocalizationService<AdminInterfaceResource>
{
}
