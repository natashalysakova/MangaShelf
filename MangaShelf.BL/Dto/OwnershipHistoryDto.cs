using MangaShelf.Common.Interfaces;

namespace MangaShelf.BL.Dto;

public class OwnershipHistoryDto
{
    public Guid Id { get; set; }
    public VolumeStatus Status { get; set; }
    public DateTimeOffset Date { get; set; }
}
