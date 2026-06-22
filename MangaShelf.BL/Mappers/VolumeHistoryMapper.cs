using MangaShelf.BL.Services;
using MangaShelf.DAL.Models;
using Riok.Mapperly.Abstractions;

namespace MangaShelf.BL.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
[UseStaticMapper(typeof(SeriesMapper))]
[UseStaticMapper(typeof(VolumeMapper))]
public static partial class VolumeHistoryMapper
{
    [MapPropertyFromSource(nameof(VolumeHistoryDto.FullVolumeName), Use = nameof(GetFullVolumeName))]
    [MapPropertyFromSource(nameof(VolumeHistoryDto.Date), Use = nameof(MapDateTimeOffsetToString))]
    public static partial VolumeHistoryDto ToDto(this VolumeHistory history);

    private static DateTime MapDateTimeOffsetToString(VolumeHistory history)
    {
        return history.Timestamp.DateTime;
    }

    static string GetFullVolumeName(VolumeHistory volume)
    {
        return volume.Volume!.GetFullVolumeName();
    }
}
