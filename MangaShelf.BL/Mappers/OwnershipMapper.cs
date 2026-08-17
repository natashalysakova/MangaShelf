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
        if (volume.Volume is null)
        {
            throw new InvalidOperationException("Ownership volume details are required to map a user volume card.");
        }

        var volumeDetails = volume.Volume;
        var reading = readings.OrderByDescending(r => r.StartedAt).FirstOrDefault(r => r.VolumeId == volume.VolumeId);
        var isLiked = volumeDetails.Likes.Any(l => l.UserId == volume.UserId);
        return new UserVolumeCard()
        {
            PublicId = volumeDetails.PublicId,
            VolumeId = volumeDetails.Id,
            CurrentOwnershipStatus = volume.Status,
            Number = volumeDetails.Number,
            ReleaseDate = volumeDetails.ReleaseDate,
            SeriesTitle = volumeDetails.Series.Title,
            VolumeTitle = volumeDetails.Title,
            CoverImageUrlSmall = volumeDetails.CoverImageUrlSmall,
            CurrentReadingStatus = reading?.Status ?? ReadingStatus.None,
            UserRating = readings.Where(r => r.Rating.HasValue && r.VolumeId == volume.VolumeId).Average(r => r.Rating),
            IsLiked = isLiked
        };
    }
}