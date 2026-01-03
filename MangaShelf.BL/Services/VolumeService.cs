using System.Linq.Expressions;
using MangaShelf.BL.Dto;
using MangaShelf.BL.Interfaces;
using MangaShelf.BL.Mappers;
using MangaShelf.Common;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL;
using MangaShelf.DAL.DomainServices;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Services;

public class VolumeService(ILogger<VolumeService> logger, IDbContextFactory<MangaDbContext> dbContextFactory) : IVolumeService
{
    public async Task<(IEnumerable<CardVolumeDto>, int)> GetAllVolumesAsync(IFilterOptions? paginationOptions = default, CancellationToken token = default)
    {
        using var context = dbContextFactory.CreateDbContext();

        var result = context.Volumes
            .Include(v => v.Series)
                .ThenInclude(s => s!.Authors)
            .Include(v => v.Series)
                .ThenInclude(s => s!.Publisher)
            .Include(v => v.Likes)
            .Where(x => x.IsPublishedOnSite)
            .Filter(paginationOptions);

        var resultList = await result.ApplyPagination(paginationOptions).ToListAsync(token);

        var totalPages = await result.GetTotalPagesAsync(paginationOptions);

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

    public async Task<(IEnumerable<Volume>, int)> GetAllFullVolumesAsync(IFilterOptions paginationOptions, IEnumerable<Func<Volume, bool>>? filterFunctions, IEnumerable<SortDefinitions<Volume>> sortDefinitions)
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

    public async Task<IEnumerable<string>> GetAllTitlesAsync(CancellationToken stoppingToken)
    {
        using var context = dbContextFactory.CreateDbContext();

        var titles = context.Volumes
            .AsNoTracking()
            .OrderBy(v => v.Title)
            .Select(v => v.Title);

        return await titles.ToListAsync(stoppingToken);
    }

    public async Task<UserVolumeStatus> GetVolumeUserStatusAsync(Guid volumeId, string userId, CancellationToken token = default)
    {
        var context = dbContextFactory.CreateDbContext();
        var user = await context.Users
            .Include(x => x.OwnedVolumes)
            .Include(x => x.Readings)
            .Include(x => x.Likes)
        .FirstOrDefaultAsync(u => u.IdentityUserId == userId);

        if (user == null)
        {
            throw new Exception("User not found");
        }

        var status = new UserVolumeStatus
        {
            IsInWishlist = IsInWishlist(user, volumeId),
            LikeStatus = GetLikeStatus(user, volumeId),
            ReadingStatus = GetReadingStatus(user, volumeId),
            Rating = GetRating(user, volumeId),
            OwnersipStatus = GetOwnershipStatus(user, volumeId)
        };

        return status;
    }

    private string GetOwnershipStatus(User user, Guid volumeId)
    {
        var ownership = user.OwnedVolumes.FirstOrDefault(o => o.VolumeId == volumeId);
        return ownership?.Status.ToString() ?? "None";
    }

    private int GetRating(User user, Guid volumeId)
    {
        var reading = user.Readings.FirstOrDefault(r => r.VolumeId == volumeId);
        return reading?.Rating ?? 0;
    }

    private LikeStatus GetLikeStatus(User user, Guid volumeId)
    {
        var like = user.Likes.FirstOrDefault(l => l.VolumeId == volumeId);
        return like?.Status ?? LikeStatus.None;
    }

    private string GetReadingStatus(User user, Guid volumeId)
    {
        var reading = user.Readings.FirstOrDefault(r => r.VolumeId == volumeId);
        return reading?.Status.ToString() ?? "None";
    }

    private bool IsInWishlist(User user, Guid volumeId)
    {
        var ownership = user.OwnedVolumes.FirstOrDefault(o => o.VolumeId == volumeId);
        return ownership != null && ownership.Status == Ownership.VolumeStatus.Wishlist;
    }
}

public class SortDefinitions<T>
{
    public Func<T, object> SortFunction { get; set; }
    public bool Descending { get; set; }
}
