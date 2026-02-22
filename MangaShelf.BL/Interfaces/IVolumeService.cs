using MangaShelf.BL.Dto;
using MangaShelf.BL.Services;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;

namespace MangaShelf.BL.Interfaces;

public interface IVolumeService : IService
{
    Task<bool> DeleteVolume(Guid volumeId, CancellationToken token = default);
    Task<bool> ChangePublishedStatus(Guid volumeId, CancellationToken token = default);
    Task<Volume?> GetFullVolumeByIdAsync(Guid volumeId, CancellationToken token = default);
    
    Task<(IEnumerable<Volume>, int)> GetAllFullVolumesAsync(IFilterOptions paginationOptions, IEnumerable<Func<Volume, bool>>? filterFunctions, IEnumerable<SortDefinitions<Volume>> sortDefinitions, bool showDeleted = false, CancellationToken token = default);

    Task<IEnumerable<string>> FilterExistingVolumes(IEnumerable<string> volumesToParse, CancellationToken token = default);
    Task<IEnumerable<string>> GetAllTitlesAsync(CancellationToken stoppingToken);
    
    Task<(IEnumerable<CardVolumeDto>, int)> GetAllVolumesAsync(IFilterOptions? paginationOptions = default, CancellationToken token = default);
    Task<(IEnumerable<CardVolumeDto>, int, IEnumerable<UserVolumeStatusDto>)> GetAllVolumesAsyncWithUserInfo(string? userId, IFilterOptions? paginationOptions = default, CancellationToken token = default);
    Task<IEnumerable<CardVolumeDto>> GetLatestPreorders(int count = 6, CancellationToken token = default);
    Task<IEnumerable<CardVolumeDto>> GetNewestReleases(int count = 6, CancellationToken token = default);

    Task<IEnumerable<CardVolumeDto>> GetVolumesBySeriesId(Guid seriesId, CancellationToken token = default);

    Task<VolumeDto?> GetFullVolumeByPublicIdAsync(string volumePublicId, CancellationToken token = default);
    Task<UserVolumeStatusDto> GetVolumeStatusInfo(Guid volumeId, string userIdentityId, CancellationToken token = default);
    Task<VolumeStatsDto> GetVolumeStats(Guid volumeId, CancellationToken token = default);

    Task<(UserVolumeStatusDto, VolumeStatsDto)> ToggleWishlist(Guid volumeId, string userIdentityId, CancellationToken token = default);
    Task<(UserVolumeStatusDto, VolumeStatsDto)> ToggleLike(Guid volumeId, string userIdentityId, CancellationToken token = default);

    Task<(UserVolumeStatusDto?, VolumeStatsDto)> AddEditOwnershipAsync(Ownership ownership, string userIdentityId, CancellationToken token = default);
    Task<(UserVolumeStatusDto, VolumeStatsDto)> RemoveOwnershipAsync(Guid ownershipId, CancellationToken token = default);

    Task<(UserVolumeStatusDto, VolumeStatsDto)> AddEditReadingAsync(Reading reading, string userIdentityId, CancellationToken token = default);
    Task<(UserVolumeStatusDto, VolumeStatsDto)> RemoveReadingAsync(Guid readingId, CancellationToken token = default);


    Task<IEnumerable<ReviewDto>> GetReviews(Guid volumeId, CancellationToken token = default);
}
