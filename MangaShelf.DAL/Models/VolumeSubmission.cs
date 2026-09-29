namespace MangaShelf.DAL.Models;

public class VolumeSubmission : BaseEntity
{
    public required string SubmittedByIdentityUserId { get; set; }
    public VolumeSubmissionStatus Status { get; set; } = VolumeSubmissionStatus.Pending;

    public Guid? SeriesId { get; set; }
    public string? ExistingSeriesTitle { get; set; }
    public string? ExistingPublisherName { get; set; }
    public string? NewSeriesTitle { get; set; }
    public string? NewSeriesOriginalTitle { get; set; }
    public SeriesType NewSeriesType { get; set; }
    public SeriesStatus NewSeriesStatus { get; set; }
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
    public required DateTimeOffset ReleaseDate { get; set; }
    public VolumeType Type { get; set; }
    public bool SingleIssue { get; set; }

    public Guid? ApprovedVolumeId { get; set; }
    public string? ReviewedByIdentityUserId { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string? ReviewComment { get; set; }
}

public enum VolumeSubmissionStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
