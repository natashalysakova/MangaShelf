using MangaShelf.BL.Dto;
using MangaShelf.BL.Services;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MangaShelf.BL.Interfaces;

public interface IVolumeService : IService
{
    Task<bool> ChangePublishedStatus(Guid volumeId, CancellationToken token = default);
    Task<Volume?> GetFullVolumeByIdAsync(Guid id, CancellationToken token = default);
    
    Task<(IEnumerable<Volume>, int)> GetAllFullVolumesAsync(IFilterOptions paginationOptions, IEnumerable<Func<Volume, bool>>? filterFunctions, IEnumerable<SortDefinitions<Volume>> sortDefinitions);

    Task<IEnumerable<string>> FilterExistingVolumes(IEnumerable<string> volumesToParse, CancellationToken token = default);
    Task<IEnumerable<string>> GetAllTitlesAsync(CancellationToken stoppingToken);
    
    Task<(IEnumerable<CardVolumeDto>, int)> GetAllVolumesAsync(IFilterOptions? paginationOptions = default, CancellationToken token = default);
    Task<IEnumerable<CardVolumeDto>> GetLatestPreorders(int count = 6, CancellationToken token = default);
    Task<IEnumerable<CardVolumeDto>> GetNewestReleases(int count = 6, CancellationToken token = default);

    Task<IEnumerable<CardVolumeDto>> GetVolumesBySeriesId(string seriesPublicId, CancellationToken token = default);

    Task<VolumeDto?> GetFullVolumeByPublicIdAsync(string volumePublicId, CancellationToken token = default);
    Task<UserVolumeStatusDto> GetVolumeStatusInfo(string volumePublicId, string userId, CancellationToken token = default);
    Task<VolumeStatsDto> GetVolumeStats(string volumePublicId, CancellationToken token = default);
    Task<(UserVolumeStatusDto, VolumeStatsDto)> ToggleWishlist(string publicVolumeId, string userId);
    Task<(UserVolumeStatusDto, VolumeStatsDto)> ToggleLike(string publicVolumeId, string userId);

    Task<(UserVolumeStatusDto, VolumeStatsDto)> AddOwnershipAsync(string volumePublicId, string userId, Ownership ownership);
    Task<(UserVolumeStatusDto, VolumeStatsDto)> RemoveOwnershipAsync(Guid ownershipId);
    Task<IEnumerable<ReviewDto>> GetReviews(string volumePublicId);
}
