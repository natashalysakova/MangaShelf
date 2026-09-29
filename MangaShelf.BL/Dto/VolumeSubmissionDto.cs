using MangaShelf.DAL.Models;

namespace MangaShelf.BL.Dto;

public class VolumeSubmissionRequestDto
{
    public Guid? SeriesId { get; set; }
    public string? NewSeriesTitle { get; set; }
    public string? NewSeriesOriginalTitle { get; set; }
    public SeriesType NewSeriesType { get; set; } = SeriesType.Manga;
    public SeriesStatus NewSeriesStatus { get; set; } = SeriesStatus.Unknown;
    public int? NewSeriesTotalVolumes { get; set; }

    public Guid? PublisherId { get; set; }
    public string? NewPublisherName { get; set; }
    public string? NewPublisherUrl { get; set; }
    public Guid? NewPublisherCountryId { get; set; }

    public string? Title { get; set; }
    public int? Number { get; set; }
    public string? ISBN { get; set; }
    public int AgeRestriction { get; set; }
    public string? PurchaseUrl { get; set; }
    public string? Description { get; set; }
    public bool IsPreorder { get; set; }
    public DateTimeOffset? PreorderStart { get; set; }
    public DateTimeOffset ReleaseDate { get; set; }
    public VolumeType Type { get; set; } = VolumeType.Physical;
    public bool SingleIssue { get; set; }
    public string? CoverImageUrl { get; set; }
}

public class VolumeSubmissionDto : VolumeSubmissionRequestDto
{
    public Guid Id { get; set; }
    public string SubmittedByIdentityUserId { get; set; } = string.Empty;
    public VolumeSubmissionStatus Status { get; set; }
    public string SeriesTitle { get; set; } = string.Empty;
    public string PublisherName { get; set; } = string.Empty;
    public Guid? ApprovedVolumeId { get; set; }
    public string? ReviewComment { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
}
