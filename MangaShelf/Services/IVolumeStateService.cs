using MangaShelf.BL.Dto;

namespace MangaShelf.Services;

public interface IVolumeStateService
{
    event Action? OnStatsChanged;
    event Action? OnUserStatusChanged;
    VolumeStatsDto? CurrentStats { get; }
    UserVolumeStatusDto? CurrentUserStatus { get; }
    Task InitializeAsync(Guid volumeId, string? userId = null);
    void UpdateStats(VolumeStatsDto stats);
    void UpdateUserStatus(UserVolumeStatusDto userStatus);
    void UpdateBoth(UserVolumeStatusDto userStatus, VolumeStatsDto stats);
    void Clear();
}
