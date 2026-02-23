using MangaShelf.DAL.Models;
using System.Diagnostics.CodeAnalysis;
using static MangaShelf.DAL.Models.Ownership;

namespace MangaShelf.BL.Dto;

[ExcludeFromCodeCoverage]
public class UserVolumeStatusDto
{
    public required Guid VolumeId { get; set; }
    public bool IsLiked { get; set; }
    public bool IsInWishlist { get; set; }

    public VolumeStatus CurrentOwnershipStatus { get; set; }
    public IEnumerable<OwnershipHistoryDto> Ownerships { get; set; } = [];
    public IEnumerable<ReadingHistoryDto> Readings { get; set; } = [];
}

public class OwnershipHistoryDto
{
    public Guid Id { get; set; }
    public VolumeStatus Status { get; set; }
    public DateTimeOffset Date { get; set; }
}

public class ReadingHistoryDto
{
    public Guid Id { get; set; }
    public ReadingStatus Status { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public int? Rating { get; set; }
    public string? ReviewId { get; set; }
}