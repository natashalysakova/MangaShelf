using MangaShelf.BL.Dto;
using MangaShelf.DAL.Models;
using Riok.Mapperly.Abstractions;

namespace MangaShelf.BL.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
[UseStaticMapper(typeof(UserMapper))]
public static partial class UserMapper
{
    [MapPropertyFromSource(nameof(UserVolumeStatusDto.Ownerships), Use = nameof(GetOwnerships))]
    [MapPropertyFromSource(nameof(UserVolumeStatusDto.Readings), Use = nameof(GetReadings))]
    [MapPropertyFromSource(nameof(UserVolumeStatusDto.IsInWishlist), Use = nameof(IsInWishlist))]
    [MapPropertyFromSource(nameof(UserVolumeStatusDto.IsLiked), Use = nameof(IsLiked))]
    [MapPropertyFromSource(nameof(UserVolumeStatusDto.CurrentOwnershipStatus), Use = nameof(GetCurrentOwnershipStatus))]
    public static partial UserVolumeStatusDto ToUserVolumeStatusDto(this User reading);

    private static VolumeStatus GetCurrentOwnershipStatus(User user)
    {
        var currentOwnership = user.OwnedVolumes
            .Where(x=>x.Status != VolumeStatus.Wishlist)
            .OrderByDescending(o => o.Date)
            .FirstOrDefault();

        if(currentOwnership == null)
        {
            return VolumeStatus.None;
        }

        return currentOwnership.Status;
    }

    private static IEnumerable<OwnershipHistoryDto> GetOwnerships(User user)
    {
        return user.OwnedVolumes.Select(x => x.ToOwnershipHistoryDto());
    }

    private static IEnumerable<ReadingHistoryDto> GetReadings(User user)
    {
        return user.Readings.Select(x => x.ToReadingHistoryDto());
    }

    private static bool IsInWishlist(User user)
    {
        return user.OwnedVolumes.Any(o => o.Status == VolumeStatus.Wishlist);
    }
    private static bool IsLiked(User user)
    {
        return user.Likes.Any(x=>x.Status == LikeStatus.Liked);
    }
}
