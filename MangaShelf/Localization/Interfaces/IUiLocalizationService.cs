using MangaShelf.Common.Localization.Interfaces;
using MangaShelf.Localization.Resources;
using MangaShelf.Localization.Resources.Components;

namespace MangaShelf.Localization.Interfaces;

public interface IUiLocalizationService : ILocalizationService<UserInterfaceResource>
{
}

public interface IVolumePageLocalizationService : ILocalizationService<VolumePageResource>
{
}
