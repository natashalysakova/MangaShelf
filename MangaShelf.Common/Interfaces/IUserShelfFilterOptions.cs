namespace MangaShelf.Common.Interfaces;

public interface IUserShelfFilterOptions
{
    string? SearchTerm { get; set; }
    IEnumerable<VolumeStatus> CurrentOwnershipStatuses { get; set; }
    IEnumerable<ReadingStatus> CurrentReadingStatuses { get; set; }
    bool? IsLiked { get; set; }
    bool? IsInWishlist { get; set; }

    int? MinRating { get; set; }
    int? MaxRating { get; set; }
}
