namespace MangaShelf.DAL.Models;

public class Series : BaseEntity
{
    public required string Title { get; set; }
    public string? OriginalName { get; set; }
    public ICollection<string> Aliases { get; set; } = new List<string>();
    public int MalId { get; set; }

    public SeriesType Type { get; set; }
    public SeriesStatus Status { get; set; }

    public int? TotalVolumes { get; set; }

    public bool IsPublishedOnSite { get; set; }

    public Guid PublisherId { get; set; }
    public virtual Publisher? Publisher { get; set; }

    public virtual ICollection<Author> Authors { get; set; } = new List<Author>();
    public virtual ICollection<Volume> Volumes { get; set; } = new List<Volume>();
}

public enum SeriesStatus
{
    Unknown = 0,
    Ongoing = 1,
    Completed = 2,
    Hiatus = 3,
    Cancelled = 4,
    OneShot = 5
}

public enum SeriesType
{
    Unknown = 0,
    Manga = 1, 
    Manhwa = 2,
    Manhua = 3,
    Novel = 4,
    OEL = 5,
    Comic = 6,
    Artbook = 7,
    Other = 8
}
