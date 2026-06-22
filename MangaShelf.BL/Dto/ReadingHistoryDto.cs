using MangaShelf.Common.Interfaces;

namespace MangaShelf.BL.Dto;

public class ReadingHistoryDto
{
    public Guid Id { get; set; }
    public ReadingStatus Status { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public int? Rating { get; set; }
    public string? ReviewId { get; set; }
}