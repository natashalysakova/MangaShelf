using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;
using System.Diagnostics.CodeAnalysis;

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
