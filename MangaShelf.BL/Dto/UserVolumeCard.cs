using MangaShelf.Common.Interfaces;

namespace MangaShelf.BL.Dto;

public class UserVolumeCard
{
    public string PublicId { get; set; } = string.Empty;
    public int? Number { get; set; }
    public DateTimeOffset? ReleaseDate { get; set; }

    public string SeriesTitle { get; set; } = string.Empty;
    public string VolumeTitle { get; set; } = string.Empty;

    public double? UserRating { get; set; }

    public string? CoverImageUrlSmall { get; set; }

    public VolumeStatus CurrentOwnershipStatus { get; set; }
    public ReadingStatus CurrentReadingStatus { get; set; }
    public bool IsLiked { get; set; }
}