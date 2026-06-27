using MangaShelf.BL.Dto;
using MangaShelf.BL.Services;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;

namespace MangaShelf.BL.Contracts;

public interface IVolumeService : IService
{
    Task<bool> DeleteVolume(Guid volumeId, CancellationToken token = default);
    Task<bool> ChangePublishedStatus(Guid volumeId, CancellationToken token = default);
    Task<Volume?> GetFullVolumeByIdAsync(Guid volumeId, CancellationToken token = default);

    Task<(IEnumerable<Volume>, int)> GetAllFullVolumesAsync(IFilterOptions paginationOptions, IEnumerable<Func<Volume, bool>>? filterFunctions, IEnumerable<SortDefinitions<Volume>> sortDefinitions, bool showDeleted = false, CancellationToken token = default);
    Task<VolumeEditDto> GetFullVolumeForEdit(Guid volumeId, CancellationToken token = default);
    Task<VolumeEditDto> Update(VolumeEditDto volume, CancellationToken token = default);
    Task<IEnumerable<string>> FilterExistingVolumes(IEnumerable<string> volumesToParse, CancellationToken token = default);
    Task<IEnumerable<string>> GetAllTitlesAsync(CancellationToken stoppingToken);

    Task<(IEnumerable<CardVolumeDto>, int)> GetAllVolumesAsync(IFilterOptions? paginationOptions = default, CancellationToken token = default);
    Task<(IEnumerable<CardVolumeDto>, int, IEnumerable<UserVolumeStatusDto>)> GetAllVolumesAsyncWithUserInfo(string? userId, IFilterOptions? paginationOptions = default, CancellationToken token = default);
    Task<IEnumerable<CardVolumeDto>> GetLatestPreorders(int count = 6, CancellationToken token = default);
    Task<IEnumerable<CardVolumeDto>> GetNewestReleases(int count = 6, CancellationToken token = default);

    Task<IEnumerable<CardVolumeDto>> GetVolumesBySeriesId(Guid seriesId, CancellationToken token = default);
    Task<CardVolumeDto> GetVolumeCardById(Guid volumeId, CancellationToken token = default);
    Task<VolumeDto?> GetFullVolumeByPublicIdAsync(string volumePublicId, CancellationToken token = default);
    Task<UserVolumeStatusDto> GetVolumeStatusInfo(Guid volumeId, string userIdentityId, CancellationToken token = default);
    Task<VolumeStatsDto> GetVolumeStats(Guid volumeId, CancellationToken token = default);
    Task<IEnumerable<UserVolumeCard>> GetUserVolumes(string userIdentityId, IUserShelfFilterOptions filterOptions, CancellationToken token = default);

    Task<(UserVolumeStatusDto, VolumeStatsDto)> ToggleWishlist(Guid volumeId, string userIdentityId, CancellationToken token = default);
    Task<(UserVolumeStatusDto, VolumeStatsDto)> ToggleLike(Guid volumeId, string userIdentityId, CancellationToken token = default);

    Task<(UserVolumeStatusDto?, VolumeStatsDto)> AddEditOwnershipAsync(Ownership ownership, string userIdentityId, CancellationToken token = default);
    Task<(UserVolumeStatusDto, VolumeStatsDto)> RemoveOwnershipAsync(Guid ownershipId, CancellationToken token = default);

    Task<(UserVolumeStatusDto, VolumeStatsDto)> AddEditReadingAsync(Reading reading, string userIdentityId, CancellationToken token = default);
    Task<(UserVolumeStatusDto, VolumeStatsDto)> RemoveReadingAsync(Guid readingId, CancellationToken token = default);


    Task<VolumeCoverDto> UpdateImages(VolumeCoverDto volumeCover, CancellationToken token = default);

    Task<IEnumerable<ReviewDto>> GetReviews(Guid volumeId, CancellationToken token = default);
    Task<Reading?> GetReading(Guid id, CancellationToken token = default);
    Task<Ownership?> GetOwnership(Guid id, CancellationToken token = default);
}

public class VolumeCoverDto
{
    public required Guid Id { get; set; }
    public required string PublicId { get; set; }
    public string? OriginalCover { get; set; } 
    public string? SmallCover { get; set; }
    public string? CroppedCover { get; set; }
}
