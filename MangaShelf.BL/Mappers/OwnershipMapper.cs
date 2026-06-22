using MangaShelf.BL.Dto;
using MangaShelf.BL.Services;
using MangaShelf.Common.Interfaces;
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
                Title = ownership.Volume!.GetFullVolumeName(),
                VolumeStatus = ownership.Status,
                ReleaseDate = ownership.Volume.ReleaseDate,
                DaysTillRelease = (ownership.Volume.ReleaseDate - DateTimeOffset.UtcNow).Days,
                CoverUrl = ownership.Volume.CoverImageUrlSmall
            };
    }

        public static UserVolumeCard ToUserVolumeCard(this Ownership volume, IEnumerable<Reading> readings)
    {
        var reading = readings.OrderByDescending(r => r.StartedAt).FirstOrDefault(r => r.VolumeId == volume.VolumeId);
        return new UserVolumeCard()
        {
            PublicId = volume.Volume.PublicId,
            CurrentOwnershipStatus = volume.Status,
            Number = volume.Volume.Number,
            SeriesTitle = volume.Volume.Series.Title,
            CoverImageUrlSmall = volume.Volume.CoverImageUrlSmall,
            CurrentReadingStatus = reading?.Status ?? ReadingStatus.None,
            UserRating = readings.Where(r => r.Rating.HasValue).Average(r => r.Rating)
        };
    }
}