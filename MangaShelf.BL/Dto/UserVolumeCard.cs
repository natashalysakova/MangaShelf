using MangaShelf.Common.Interfaces;

namespace MangaShelf.BL.Dto;

public class UserVolumeCard
{
    public string PublicId { get; set; }
    public int? Number { get; set; }

    public string SeriesTitle { get; set; }

    public double? UserRating { get; set; }

    public string? CoverImageUrlSmall { get; set; }

    public VolumeStatus CurrentOwnershipStatus { get; set; }
    public ReadingStatus CurrentReadingStatus { get; set; }
}