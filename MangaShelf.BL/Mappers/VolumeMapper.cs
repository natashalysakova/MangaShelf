using MangaShelf.BL.Dto;
using MangaShelf.DAL.Models;
using Riok.Mapperly.Abstractions;

namespace MangaShelf.BL.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
[UseStaticMapper(typeof(SeriesMapper))]
public static partial class VolumeMapper
{
    [MapProperty(nameof(Volume.Series.Title), nameof(CardVolumeDto.SeriesName))]
    public static partial CardVolumeDto ToDto(this Volume volume);


    private static string? MapDateTimeOffsetToString(DateTimeOffset? source) => source?.ToString("dd.MM.yyyy");

    [MapPropertyFromSource(nameof(VolumeDto.Stats), Use = nameof(MapVolumeStats))]
    public static partial VolumeDto ToFullDto(this Volume volume);

    private static VolumeStats MapVolumeStats(Volume volume)
    {
        return new VolumeStats
        {
            OwnersCount = volume.Owners.Count(x => x is
            {
                Status:
                    Ownership.VolumeStatus.Own or
                    Ownership.VolumeStatus.Preorder
            }),
            WishlistsCount = volume.Owners.Count(o => o.Status == Ownership.VolumeStatus.Wishlist),
            ReadersCount = volume.Readers.Count(x => x.Status is Reading.ReadingStatus.Reading),
            CompletedCount = volume.Readers.Count(r => r.Status == Reading.ReadingStatus.Completed)
        };
    }
}
