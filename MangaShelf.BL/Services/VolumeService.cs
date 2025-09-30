using MangaShelf.BL.Dto;
using MangaShelf.BL.Interfaces;
using MangaShelf.BL.Mappers;
using MangaShelf.Common;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Services;

public class VolumeService(ILogger<VolumeService> logger, IDbContextFactory<MangaDbContext> dbContextFactory) : IVolumeService
{
    public async Task<(IEnumerable<CardVolumeDto>, int)> GetAllVolumesAsync(IPaginationOptions? paginationOptions = default, CancellationToken token = default)
    {
        using var context = dbContextFactory.CreateDbContext();

        var result = context.Volumes
            .Include(v => v.Series)
                .ThenInclude(s => s.Authors)
            .Where(x => x.IsPublishedOnSite);

        if (!string.IsNullOrEmpty(paginationOptions.Search))
        {
            result = result.Where(x =>
                EF.Functions.Like(x.Title, $"%{paginationOptions.Search}%") ||
                EF.Functions.Like(x.Series.Title, $"%{paginationOptions.Search}%") ||
                x.Series.Authors.Any(a => EF.Functions.Like(a.Name, $"%{paginationOptions.Search}%")));
        }

        var totalPages = 1;

        if (paginationOptions.Take != 0)
        {
            var totalCount = await result.CountAsync();
            var pagesCount = (double)totalCount / paginationOptions.Take;
            totalPages = (int)Math.Ceiling(pagesCount);

            result = result.Skip(paginationOptions.Skip).Take(paginationOptions.Take);
        }

        var resultList = await result.ToListAsync(token);

        return (resultList.Select(v => v.ToDto()), totalPages);
    }

    public async Task<IEnumerable<string>> FilterExistingVolumes(IEnumerable<string> volumesToParse, CancellationToken token = default)
    {
        if (volumesToParse == null || !volumesToParse.Any())
        {
            return Enumerable.Empty<string>();
        }

        using var context = dbContextFactory.CreateDbContext();

        var existingUrls = context.Volumes
            .Where(v => v.PurchaseUrl != null && volumesToParse.Contains(v.PurchaseUrl))
            .Where(v => !v.IsPreorder)
            .Select(v => v.PurchaseUrl!);

        // Return URLs that are in volumesToParse but not in existingUrls
        return volumesToParse.Except(await existingUrls.ToListAsync(token));
    }

    public async Task<IEnumerable<CardVolumeDto>> GetLatestPreorders(int count = 6, CancellationToken token = default)
    {
        using var context = dbContextFactory.CreateDbContext();
        var volumeDomainService = new DomainServiceFactory(context).GetDomainService<IVolumeDomainService>();

        var volumes = volumeDomainService.GetLatestPreorders(count);

        return await volumes.Select(v => v.ToDto()).ToListAsync(token);
    }

    public async Task<IEnumerable<CardVolumeDto>> GetNewestReleases(int count = 6, CancellationToken token = default)
    {
        using var context = dbContextFactory.CreateDbContext();
        var volumeDomainService = new DomainServiceFactory(context).GetDomainService<IVolumeDomainService>();

        var volumes = volumeDomainService.GetNewestReleases(count);

        return await volumes.Select(v => v.ToDto()).ToListAsync(token);
    }

    public async Task<VolumeDto?> GetFullVolumeByIdAsync(Guid id, CancellationToken token = default)
    {
        using var context = dbContextFactory.CreateDbContext();
        var volumeDomainService = new DomainServiceFactory(context).GetDomainService<IVolumeDomainService>();

        var volume = await volumeDomainService.GetAllFull().FirstOrDefaultAsync(x => x.Id == id);

        return volume?.ToFullDto();
    }

    public async Task<(IEnumerable<Volume>, int)> GetAllFullVolumesAsync(IPaginationOptions paginationOptions, IEnumerable<Func<Volume, bool>>? filterFunctions, IEnumerable<SortDefinitions<Volume>> sortDefinitions)
    {
        using var context = dbContextFactory.CreateDbContext();
        var volumeDomainService = new DomainServiceFactory(context).GetDomainService<IVolumeDomainService>();


        var result = volumeDomainService.GetAllFull();
        if (sortDefinitions.Any())
        {
            var firstSort = sortDefinitions.First();

            if (firstSort.Descending)
            {
                result = result.OrderByDescending(firstSort.SortFunction).AsQueryable();
            }
            else
            {
                result = result.OrderBy(firstSort.SortFunction).AsQueryable();
            }
        }
        else
        {
            result = result
                .OrderBy(x => x.Series.Title)
                    .ThenBy(x => x.Number);
        }

        var resultList = result.ToList();

        if (filterFunctions != null && filterFunctions.Any())
        {
            resultList = resultList
                .Where(x => filterFunctions.All(f => f(x))).ToList();
        }

        var totalVolumes = resultList.Count();


        resultList = resultList
            .Skip(paginationOptions.Skip)
            .Take(paginationOptions.Take).ToList();


        return (resultList, totalVolumes);
    }
}

public class SortDefinitions<T>
{
    public Func<T, object> SortFunction { get; set; }
    public bool Descending { get; set; }
}
