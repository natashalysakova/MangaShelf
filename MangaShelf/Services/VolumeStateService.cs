using MangaShelf.BL.Dto;
using MangaShelf.BL.Interfaces;

namespace MangaShelf.Services;

/// <summary>
/// Scoped state service for managing volume-related data on the volume page.
/// Prevents duplicate API calls and keeps components synchronized.
/// </summary>
public class VolumeStateService : IVolumeStateService, IDisposable
{
    private readonly IVolumeService _volumeService;
    private readonly ILogger<VolumeStateService> _logger;

    private VolumeStatsDto? _currentStats;
    private UserVolumeStatusDto? _currentUserStatus;
    private string? _currentVolumePublicId;
    private string? _currentUserId;

    public VolumeStateService(IVolumeService volumeService, ILogger<VolumeStateService> logger)
    {
        _volumeService = volumeService;
        _logger = logger;
    }

    /// <summary>
    /// Fired when volume stats are updated (likes, wishlists, ownership counts)
    /// </summary>
    public event Action? OnStatsChanged;

    /// <summary>
    /// Fired when user's volume status is updated (user's like, wishlist, ownership)
    /// </summary>
    public event Action? OnUserStatusChanged;

    /// <summary>
    /// Current volume statistics (aggregated from all users)
    /// </summary>
    public VolumeStatsDto? CurrentStats => _currentStats;

    /// <summary>
    /// Current authenticated user's status for this volume
    /// </summary>
    public UserVolumeStatusDto? CurrentUserStatus => _currentUserStatus;

    /// <summary>
    /// Initialize state for a specific volume and user.
    /// Loads data from server if not already cached.
    /// </summary>
    public async Task InitializeAsync(string volumePublicId, string? userId = null)
    {
        // If same volume, don't reload
        if (_currentVolumePublicId == volumePublicId)
        {
            return;
        }

        Clear();
        _currentVolumePublicId = volumePublicId;
        _currentUserId = userId;

        // Load stats (public data)
        await LoadStatsAsync(volumePublicId);

        // Load user-specific data if authenticated
        if (!string.IsNullOrEmpty(userId))
        {
            await LoadUserStatusAsync(volumePublicId, userId);
        }
    }

    /// <summary>
    /// Load volume statistics from server
    /// </summary>
    private async Task LoadStatsAsync(string volumePublicId)
    {
        try
        {
            _currentStats = await _volumeService.GetVolumeStats(volumePublicId);
            NotifyStatsChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading volume stats for PublicId: {PublicId}", volumePublicId);
        }
    }

    /// <summary>
    /// Load user's volume status from server
    /// </summary>
    private async Task LoadUserStatusAsync(string volumePublicId, string userId)
    {
        try
        {
            _currentUserStatus = await _volumeService.GetVolumeStatusInfo(volumePublicId, userId);
            NotifyUserStatusChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user volume status for PublicId: {PublicId}", volumePublicId);
        }
    }

    /// <summary>
    /// Update stats after a user action (like, wishlist, ownership change)
    /// </summary>
    public void UpdateStats(VolumeStatsDto stats)
    {
        _currentStats = stats;
        NotifyStatsChanged();
    }

    /// <summary>
    /// Update user status after a user action
    /// </summary>
    public void UpdateUserStatus(UserVolumeStatusDto userStatus)
    {
        _currentUserStatus = userStatus;
        NotifyUserStatusChanged();
    }

    /// <summary>
    /// Update both stats and user status atomically (from service methods that return both)
    /// </summary>
    public void UpdateBoth(UserVolumeStatusDto userStatus, VolumeStatsDto stats)
    {
        _currentUserStatus = userStatus;
        _currentStats = stats;
        
        // Notify in order: user status first, then stats
        NotifyUserStatusChanged();
        NotifyStatsChanged();
    }

    /// <summary>
    /// Clear all state (call when navigating away from volume page)
    /// </summary>
    public void Clear()
    {
        _currentStats = null;
        _currentUserStatus = null;
        _currentVolumePublicId = null;
        _currentUserId = null;
    }

    private void NotifyStatsChanged() => OnStatsChanged?.Invoke();
    private void NotifyUserStatusChanged() => OnUserStatusChanged?.Invoke();

    public void Dispose()
    {
        Clear();
        OnStatsChanged = null;
        OnUserStatusChanged = null;
    }
}