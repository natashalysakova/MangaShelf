using MangaShelf.BL.Contracts;
using MangaShelf.BL.Dto;
using MangaShelf.DAL.Models;
using Pomelo.EntityFrameworkCore.MySql.Query.ExpressionVisitors.Internal;
using Riok.Mapperly.Abstractions;

namespace MangaShelf.BL.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
[UseStaticMapper(typeof(SeriesMapper))]
[UseStaticMapper(typeof(AuthorMapper))]
public static partial class VolumeMapper
{
    [MapProperty(nameof(Volume.Series.Title), nameof(CardVolumeDto.SeriesName))]
    [MapProperty(nameof(Volume.ReleaseDate), nameof(CardVolumeDto.ReleaseDate), Use = nameof(MapDateTimeOffsetToString))]
    public static partial CardVolumeDto ToDto(this Volume volume);


    private static string? MapDateTimeOffsetToString(DateTimeOffset source) => source.ToLocalTime().ToString("dd.MM.yyyy");
    private static DateTime? MapNullableDateTimeOffsetToLocalDateTime(DateTimeOffset? source) => source?.LocalDateTime ?? null;
    private static DateTime? MapDateTimeOffsetToLocalDateTime(DateTimeOffset source) => source.LocalDateTime;


    [MapPropertyFromSource(nameof(VolumeDto.FullTitle), Use = nameof(MapFullTitle))]
    public static partial VolumeDto ToFullDto(this Volume volume);

    private static string MapFullTitle(Volume volume)
    {
        return volume.GetFullVolumeName();
    }

    [MapProperty(nameof(VolumeEditDto.PreorderStart), nameof(Volume.PreorderStart), Use = nameof(MapNullableDateTimeOffsetToLocalDateTime))]
    [MapProperty(nameof(VolumeEditDto.ReleaseDate), nameof(Volume.ReleaseDate), Use = nameof(MapDateTimeOffsetToLocalDateTime))]
    [MapPropertyFromSource(nameof(VolumeEditDto.Cover), Use = nameof(ToCoverDto))]
    public static partial VolumeEditDto ToEditDto(this Volume volume);

    public static VolumeCoverDto ToCoverDto(this Volume volume)
    {
        return new VolumeCoverDto
        {
            Id = volume.Id,
            PublicId = volume.PublicId,
            OriginalCover = volume.OriginalCoverUrl,
            SmallCover = volume.CoverImageUrlSmall,
            CroppedCover = volume.CoverImageUrl
        };
    }
}


