using MangaShelf.BL.Dto;
using MangaShelf.DAL.Models;
using Riok.Mapperly.Abstractions;

namespace MangaShelf.BL.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
[UseStaticMapper(typeof(ReadingsMapper))]
public static partial class ReadingsMapper
{
    [MapProperty(nameof(Reading.User.VisibleUsername), nameof(ReviewDto.UserName))]
    [MapPropertyFromSource(nameof(ReviewDto.Rating), Use = nameof(MapRaiting))]
    [MapProperty(nameof(Reading.CreatedAt), nameof(ReviewDto.Date))]
    public static partial ReviewDto ToReviewDto(this Reading reading);

    private static int MapRaiting(Reading? reading) => reading.Rating ?? 0;

    [MapPropertyFromSource(nameof(ReadingHistoryDto.ReviewId), Use = nameof(MapReviewId))]
    public static partial ReadingHistoryDto ToReadingHistoryDto(this Reading reading);

    private static string? MapReviewId(Reading reading)
    {
        if((reading.Rating == null || reading.Rating == 0) && string.IsNullOrEmpty(reading.Review))
        {
            return null;
        }

        return reading.Id.ToString();
    }
}
