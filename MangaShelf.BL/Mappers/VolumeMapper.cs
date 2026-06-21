using MangaShelf.BL.Contracts;
using MangaShelf.BL.Dto;
using MangaShelf.DAL.Models;
using Riok.Mapperly.Abstractions;

namespace MangaShelf.BL.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
[UseStaticMapper(typeof(SeriesMapper))]
[UseStaticMapper(typeof(AuthorMapper))]
public static partial class VolumeMapper
{
    [MapProperty(nameof(Volume.Series.Title), nameof(CardVolumeDto.SeriesName))]
    public static partial CardVolumeDto ToDto(this Volume volume);


    private static string? MapDateTimeOffsetToString(DateTimeOffset? source) => source?.ToString("dd.MM.yyyy");
    private static DateTime? MapDateTimeOffsetToLocalDateTime(DateTimeOffset? source) => source?.LocalDateTime ?? null;
    public static partial VolumeDto ToFullDto(this Volume volume);

    [MapProperty(nameof(VolumeEditDto.PreorderStart), nameof(Volume.PreorderStart), Use = nameof(MapDateTimeOffsetToLocalDateTime))]
    [MapProperty(nameof(VolumeEditDto.ReleaseDate), nameof(Volume.ReleaseDate), Use = nameof(MapDateTimeOffsetToLocalDateTime))]
    public static partial VolumeEditDto ToEditDto(this Volume volume);
}
