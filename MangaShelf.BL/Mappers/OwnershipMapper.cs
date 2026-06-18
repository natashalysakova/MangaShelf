using MangaShelf.BL.Dto;
using MangaShelf.BL.Services;
using MangaShelf.DAL.Models;
using Riok.Mapperly.Abstractions;

namespace MangaShelf.BL.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
[UseStaticMapper(typeof(OwnershipMapper))]
public static partial class OwnershipMapper
{
    public static partial OwnershipHistoryDto ToOwnershipHistoryDto(this Ownership ownership);

    public static UserLibraryItem ToUserLibraryItemDto(this Ownership ownership)
    {
        return new UserLibraryItem()
            {
                VolumeId = ownership.VolumeId,
                VolumeTitle = ownership.Volume!.Title,
                SeriesTitle = ownership.Volume!.Series!.Title,
                VolumeStatus = ownership.Status,
                ReleaseDate = ownership.Volume.ReleaseDate,
                DaysTillRelease = ownership.Volume.ReleaseDate.HasValue ? (ownership.Volume.ReleaseDate.Value - DateTimeOffset.UtcNow).Days : 0,
                CoverUrl = ownership.Volume.CoverImageUrlSmall
            };
    }
}