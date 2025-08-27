using MangaShelf.Common.Dto;
using MangaShelf.DAL.Models;
using Riok.Mapperly.Abstractions;

namespace MangaShelf.BL.Mappers;

[Mapper]
public static partial class CountryMapper
{
    [MapProperty(nameof(Country.CountryCode), nameof(CountryDto.Code))]
    public static partial CountryDto ToDto(this Country country);
}

[Mapper]
public static partial class VolumeMapper
{
    [MapProperty(nameof(Volume.Series.Title), nameof(VolumeDto.SeriesName))]
    [MapProperty(nameof(Volume.Title), nameof(VolumeDto.VolumeTitle))]
    [MapProperty(nameof(Volume.CoverImageUrl), nameof(VolumeDto.CoverImageUrl))]
    [MapProperty(nameof(Volume.Number), nameof(VolumeDto.VolumeNumber))]
    public static partial VolumeDto ToDto(this Volume country);
}