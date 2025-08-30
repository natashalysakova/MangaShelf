using MangaShelf.BL.Dto;
using MangaShelf.DAL.Models;
using Riok.Mapperly.Abstractions;

namespace MangaShelf.BL.Mappers;

[Mapper]
public static partial class VolumeMapper
{
    [MapProperty(nameof(Volume.Series.Title), nameof(VolumeDto.SeriesName))]
    [MapProperty(nameof(Volume.Title), nameof(VolumeDto.VolumeTitle))]
    [MapProperty(nameof(Volume.CoverImageUrl), nameof(VolumeDto.CoverImageUrl))]
    [MapProperty(nameof(Volume.Number), nameof(VolumeDto.VolumeNumber))]
    [MapProperty(nameof(Volume.PurchaseUrl), nameof(VolumeDto.Url))]
    public static partial VolumeDto ToDto(this Volume country);
}
