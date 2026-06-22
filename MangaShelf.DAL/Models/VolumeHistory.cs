namespace MangaShelf.DAL.Models;

public class VolumeHistory : BaseEntity
{
    public required Guid VolumeId { get; set; }
    public virtual Volume? Volume { get; set; }

    public DateTimeOffset Timestamp { get; set; }
    public required HistoryEventType EventType { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}

public enum HistoryEventType
{
    PreorderStarted,
    Released,
    ReleaseDateChanged,
}
