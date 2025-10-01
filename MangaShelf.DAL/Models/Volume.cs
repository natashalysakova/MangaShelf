namespace MangaShelf.DAL.Models;

public class Volume : BaseEntity
{
    public required string Title { get; set; }
    public int Number { get; set; }
    public string? ISBN { get; set; }

    public bool OneShot { get; set; }
    public bool SingleIssue { get; set; }
    public int AgeRestriction { get; set; }

    public string? CoverImageUrl { get; set; }
    public string? CoverImageUrlSmall { get; set; }
    public string? PurchaseUrl { get; set; }

    public string? Description { get; set; }

    public bool IsPreorder { get; set; }
    public DateTimeOffset? PreorderStart { get; set; }
    public DateTimeOffset? ReleaseDate { get; set; }

    public double AvgRating { get; set; }

    public bool IsPublishedOnSite { get; set; }
    public VolumeType Type { get; set; }

    public Guid SeriesId { get; set; }
    public virtual Series? Series { get; set; }

    public virtual ICollection<Author> OverrideAuthors { get; set; } = new List<Author>();
    public virtual ICollection<Ownership> Owners { get; set; } = new List<Ownership>();
    public virtual ICollection<Reading> Readers { get; set; } = new List<Reading>();
}

[Flags]
public enum VolumeType
{
    Physical = 1,
    Digital = 2,
}