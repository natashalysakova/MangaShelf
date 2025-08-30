using MangaShelf.BL.Dto;
using MangaShelf.BL.Interfaces;
using MangaShelf.BL.Mappers;
using MangaShelf.Common;
using MangaShelf.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Services;

public class VolumeService(ILogger<VolumeService> logger, IVolumeDomainService volumeRepository, ISeriesService seriesService) : IVolumeService
{
    public async Task<(IEnumerable<VolumeDto>, int)> GetAllVolumesAsync(PaginationOptions? paginationOptions = null)
    {
        var result = await volumeRepository.GetAllWithSeries(paginationOptions);
        return (result.volumes.Select(v => v.ToDto()), result.totalPages);
    }

    public async Task<VolumeDto?> GetbyIdAsync(Guid id)
    {
        var volume = await volumeRepository.Get(id);
        return volume?.ToDto();
    }

    public async Task<IEnumerable<string>> FilterExistingVolumes(IEnumerable<string> volumesToParse)
    {
        if (volumesToParse == null || !volumesToParse.Any())
        {
            return Enumerable.Empty<string>();
        }

        var existingUrls = await volumeRepository
            .GetAll()
            .Where(v => v.PurchaseUrl != null && volumesToParse.Contains(v.PurchaseUrl))
            .Where(v => !v.IsPreorder)
            .Select(v => v.PurchaseUrl!)
            .ToListAsync();

        // Return URLs that are in volumesToParse but not in existingUrls
        return volumesToParse.Except(existingUrls);
    }

    public async Task<IEnumerable<VolumeDto>> GetLatestPreorders(int count = 6)
    {
        var volumes = await volumeRepository.GetLatestPreorders(count);
        return volumes.Select(v => v.ToDto());
    }

    public async Task<IEnumerable<VolumeDto>> GetNewestReleases(int count = 6)
    {
        var volumes = await volumeRepository.GetNewestReleases(count);
        return volumes.Select(v => v.ToDto());
    }
}
