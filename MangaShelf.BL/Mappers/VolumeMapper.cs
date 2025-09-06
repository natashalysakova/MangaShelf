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

    public static partial VolumeDto ToFullDto(this Volume volume);
}
