using MangaShelf.DAL.Models;

namespace MangaShelf.BL.Services;

public class VolumeHistoryDto
{
    public string VolumePublicId { get; set; }
    public string FullVolumeName { get; set; }
    public DateTime Date { get; set; }
    public HistoryEventType EventType { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}