using MangaShelf.Common.Interfaces;

namespace MangaShelf.BL.Services;

public class UserShelfFilterOptions : IUserShelfFilterOptions
{
    public string? SearchTerm { get; set; }
    public IEnumerable<VolumeStatus> CurrentOwnershipStatuses { get; set; }
    public IEnumerable<ReadingStatus> CurrentReadingStatuses { get; set; }
    public bool? IsLiked { get; set; }
    public bool? IsInWishlist { get; set; }
    public int? MinRating { get; set; }
    public int? MaxRating { get; set; }
}
