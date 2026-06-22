using MangaShelf.Common.Interfaces;

namespace MangaShelf.DAL.Models;

public class Reading : BaseEntity
{
    public Guid UserId { get; set; }
    public virtual User? User { get; set; }
    public Guid VolumeId { get; set; }
    public virtual Volume? Volume { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public ReadingStatus Status { get; set; }
    public int? Rating { get; set; }
    public string? Review { get; set; }
}


