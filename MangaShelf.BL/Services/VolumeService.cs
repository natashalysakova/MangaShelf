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
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using static MangaShelf.DAL.Models.Ownership;
using static MangaShelf.DAL.Models.Reading;

namespace MangaShelf.BL.Services;

public class VolumeService(ILogger<VolumeService> logger, IDbContextFactory<MangaDbContext> dbContextFactory, IImageManager imageManager) : IVolumeService
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

    public async Task<VolumeDto?> GetFullVolumeByPublicIdAsync(string volumePublicId, CancellationToken token = default)
    {
        using var context = dbContextFactory.CreateDbContext();
        var volumeDomainService = new DomainServiceFactory(context).GetDomainService<IVolumeDomainService>();

        var volume = await volumeDomainService.GetAllFull().FirstOrDefaultAsync(x => x.PublicId == volumePublicId);
        if(volume == null)
        {
            
        }


        return volume!.ToFullDto();
    }

    public async Task<(IEnumerable<Volume>, int)> GetAllFullVolumesAsync(IFilterOptions paginationOptions, IEnumerable<Func<Volume, bool>>? filterFunctions, IEnumerable<SortDefinitions<Volume>> sortDefinitions, bool showDeleted = false, CancellationToken token = default)
    {
        using var context = dbContextFactory.CreateDbContext();
        var volumeDomainService = new DomainServiceFactory(context).GetDomainService<IVolumeDomainService>();


        var result = volumeDomainService.GetAllFull();
        if(showDeleted)
        {
            result = result.IgnoreQueryFilters().Where(x => x.IsDeleted);
        }

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

        var resultList = await result.ToListAsync();

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

    public async Task<UserVolumeStatusDto> GetVolumeStatusInfo(Guid volumeId, string? userId, CancellationToken token = default)
    {
        using var context = dbContextFactory.CreateDbContext();

        User? user = default;

        user = await context.Users
                    .Include(x => x.OwnedVolumes.Where(x => x.VolumeId == volumeId).OrderBy(v => v.Date))
                    .Include(x => x.Readings.Where(x => x.VolumeId == volumeId).OrderBy(x => x.StartedAt))
                    .Include(x => x.Likes.Where(x => x.VolumeId == volumeId))
                .FirstOrDefaultAsync(u => u.IdentityUserId == userId);

        if (user == null)
        {
            throw new Exception("User not found");
        }

        return user.ToUserVolumeStatusDto(volumeId);
    }



    public async Task<IEnumerable<CardVolumeDto>> GetVolumesBySeriesId(Guid seriesId, CancellationToken token = default)
    {
        using var context = dbContextFactory.CreateDbContext();

        var volumes = context.Volumes
            .Include(v => v.Series)
            .Where(v => v.Series.Id == seriesId && v.IsPublishedOnSite)
            .OrderBy(v => v.Number);

        return await volumes.Select(v => v.ToDto()).ToListAsync(token);
    }

    public async Task<bool> ChangePublishedStatus(Guid volumeId, CancellationToken token = default)
    {
        using var context = dbContextFactory.CreateDbContext();

        var volume = context.Volumes.FirstOrDefault(v => v.Id == volumeId);

        volume.IsPublishedOnSite = !volume.IsPublishedOnSite;

        await context.SaveChangesAsync(token);

        return volume.IsPublishedOnSite;
    }

    public async Task<IEnumerable<ReviewDto>> GetReviews(Guid volumeId, CancellationToken token = default)
    {
        using var context = dbContextFactory.CreateDbContext();

        var readings = await context.Readings
            .Include(r => r.User)
            .Where(r => r.VolumeId == volumeId && (r.Status != ReadingStatus.None || r.Status != ReadingStatus.PlanToRead) && (!string.IsNullOrEmpty(r.Review) || (r.Rating != null && r.Rating != 0)))
            .OrderByDescending(x=>x.CreatedAt)
            .ToListAsync(token);

        return readings.Select(r => r.ToReviewDto());
    }

    public async Task<Volume?> GetFullVolumeByIdAsync(Guid id, CancellationToken token = default)
    {
        using var context = dbContextFactory.CreateDbContext();
        var volumeDomainService = new DomainServiceFactory(context).GetDomainService<IVolumeDomainService>();

        var volume = await volumeDomainService.GetAllFull().FirstOrDefaultAsync(x => x.Id == id);

        return volume;

    }

    public async Task<(UserVolumeStatusDto?, VolumeStatsDto)> ToggleWishlist(Guid volumeId, string userIdentityId, CancellationToken token = default)
    {
        using var context = dbContextFactory.CreateDbContext();

        var user = await context.Users
            .Include(u => u.OwnedVolumes)
                .ThenInclude(ov => ov.Volume)
            .FirstOrDefaultAsync(u => u.IdentityUserId == userIdentityId, token);

        if (user == null)
        {
            throw new Exception("User not found");
        }

        var wishlistRecord = user.OwnedVolumes.FirstOrDefault(ov => ov.VolumeId == volumeId && ov.Status == VolumeStatus.Wishlist);
        if (wishlistRecord != null)
        {
            wishlistRecord.IsDeleted = true;
        }
        else
        {
            var volume = await context.Volumes.FirstOrDefaultAsync(v => v.Id == volumeId, token);
            if (volume == null)
            {
                throw new Exception("Volume not found");
            }
            wishlistRecord = new Ownership
            {
                User = user,
                Volume = volume,
                Date = DateTime.Now,
                Status = VolumeStatus.Wishlist
            };
            user.OwnedVolumes.Add(wishlistRecord);
        }

        await context.SaveChangesAsync(token);

        return (await GetVolumeStatusInfo(volumeId, userIdentityId, token), await GetVolumeStats(volumeId, token));
    }

    public async Task<(UserVolumeStatusDto?, VolumeStatsDto)> ToggleLike(Guid volumeId, string userIdentityId, CancellationToken token = default)
    {
        using var context = dbContextFactory.CreateDbContext();

        var user = await context.Users
            .Include(u => u.Likes)
                .ThenInclude(ov => ov.Volume)
            .FirstOrDefaultAsync(u => u.IdentityUserId == userIdentityId, token);

        if (user == null)
        {
            throw new Exception("User not found");
        }

        var likeRecord = user.Likes.FirstOrDefault(ov => ov.VolumeId == volumeId);
        if (likeRecord != null)
        {
            likeRecord.Status = likeRecord.Status switch
            {
                LikeStatus.Liked => LikeStatus.None,
                LikeStatus.None => LikeStatus.Liked,
                _ => LikeStatus.Liked,
            };
        }
        else
        {
            var volume = await context.Volumes.FirstOrDefaultAsync(v => v.Id == volumeId, token);
            if (volume == null)
            {
                throw new Exception("Volume not found");
            }

            likeRecord = new Likes
            {
                User = user,
                Volume = volume,
                Status = LikeStatus.Liked
            };
            user.Likes.Add(likeRecord);
        }

        await context.SaveChangesAsync(token);


        return (await GetVolumeStatusInfo(volumeId, userIdentityId, token), await GetVolumeStats(volumeId, token));
    }

    public async Task<(UserVolumeStatusDto?, VolumeStatsDto)> AddEditOwnershipAsync(Ownership ownership, string userIdentityId, CancellationToken token = default)
    {
        using var context = dbContextFactory.CreateDbContext();
        var volume = await context.Volumes
            .Include(x => x.Owners)
            .FirstOrDefaultAsync(v => v.Id == ownership.VolumeId);
        var user = await context.Users.FirstOrDefaultAsync(u => u.IdentityUserId == userIdentityId, token);
        if (user == null) {
            throw new Exception("User not found");
        }

        ownership.UserId = user.Id;
        context.Entry(ownership).State = ownership.Id == Guid.Empty ? EntityState.Added : EntityState.Modified;
        await context.SaveChangesAsync(token);

        return (await GetVolumeStatusInfo(volume.Id, user.IdentityUserId, token), await GetVolumeStats(volume.Id, token));
    }

    public async Task<VolumeStatsDto> GetVolumeStats(Guid volumeId, CancellationToken token = default)
    {
        using var context = dbContextFactory.CreateDbContext();

        var volume = await context.Volumes
            .Include(v => v.Likes)
            .Include(v => v.Owners)
            .Include(v => v.Readers)
            .FirstOrDefaultAsync(v => v.Id == volumeId);

        return new VolumeStatsDto
        {
            OwnersCount = volume.Owners.Count(x => x.Status is VolumeStatus.Own) - volume.Owners.Count(x => x.Status is VolumeStatus.Gone),
            PreordersCount = volume.Owners.Count(o => o.Status is VolumeStatus.Preorder),
            WishlistsCount = volume.Owners.Count(o => o.Status is VolumeStatus.Wishlist),
            ReadersCount = volume.Readers.Count(x => x.Status is ReadingStatus.Reading),
            CompletedCount = volume.Readers.Count(r => r.Status is ReadingStatus.Completed),
            LikesCount = volume.Likes.Count(r => r.Status is LikeStatus.Liked),
            AvgRating = volume.AvgRating
        };
    }

    public async Task<(UserVolumeStatusDto, VolumeStatsDto)> RemoveOwnershipAsync(Guid ownershipId, CancellationToken token = default)
    {
        using var context = dbContextFactory.CreateDbContext();

        var ownership = await context.Ownerships
            .Include(o => o.User)
            .Include(o => o.Volume)
            .FirstOrDefaultAsync(o => o.Id == ownershipId, token);
        if (ownership == null)
        {
            throw new Exception("Ownership not found");
        }

        context.Ownerships.Remove(ownership);
        await context.SaveChangesAsync();

        return (
            await GetVolumeStatusInfo(ownership.VolumeId, ownership.User.IdentityUserId, token),
            await GetVolumeStats(ownership.VolumeId, token));
    }

    public async Task<bool> DeleteVolume(Guid volumeId, CancellationToken token = default)
    {
        using var context = dbContextFactory.CreateDbContext();
        var volume = await context.Volumes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(v => v.Id == volumeId, token);
        if (volume == null)
        {
            throw new Exception("Volume not found");
        }

        if (volume.IsDeleted)
        {
            volume.IsDeleted = false;
        }
        else
        {
            context.Volumes.Remove(volume);
        }

        await context.SaveChangesAsync(token);
        return volume.IsDeleted;
    }

    public async Task<(UserVolumeStatusDto, VolumeStatsDto)> AddEditReadingAsync(Reading reading, string userIdentityId, CancellationToken token = default)
    {
        using var context = dbContextFactory.CreateDbContext();
        var volume = await context.Volumes
            .Include(x => x.Readers)
            .FirstOrDefaultAsync(v => v.Id == reading.VolumeId);
        var user = await context.Users.FirstOrDefaultAsync(u => u.IdentityUserId == userIdentityId, token);
        reading.UserId = user.Id;

        var avgRating = volume.Readers.Where(x => x.Rating != null && x.Rating.Value != 0);
        if (avgRating.Any())
        {
            volume.AvgRating = avgRating.Average(x => x.Rating!.Value);
        }

        var existingEntry = context.ChangeTracker.Entries<Reading>()
            .FirstOrDefault(e => e.Entity.Id == reading.Id);

        if (existingEntry != null)
        {
            existingEntry.State = EntityState.Detached;
        }

        context.Readings.Update(reading);
        await context.SaveChangesAsync(token);

        return (await GetVolumeStatusInfo(volume.Id, user.IdentityUserId), await GetVolumeStats(volume.Id));
    }

    public async Task<(UserVolumeStatusDto, VolumeStatsDto)> RemoveReadingAsync(Guid readingId, CancellationToken token = default)
    {
        using var context = dbContextFactory.CreateDbContext();

        var reading = await context.Readings
            .Include(o => o.User)
            .Include(o => o.Volume)
            .FirstOrDefaultAsync(o => o.Id == readingId);
        if (reading == null)
        {
            throw new Exception("Reading not found");
        }

        context.Readings.Remove(reading);
        await context.SaveChangesAsync();

        return (
            await GetVolumeStatusInfo(reading.VolumeId, reading.User.IdentityUserId),
            await GetVolumeStats(reading.VolumeId));
    }

    public async Task<(IEnumerable<CardVolumeDto>, int, IEnumerable<UserVolumeStatusDto>)> GetAllVolumesAsyncWithUserInfo(string? userId, IFilterOptions? paginationOptions = null, CancellationToken token = default)
    {
        var volumes = await GetAllVolumesAsync(paginationOptions, token);

        var userVolumeStatuses = new List<UserVolumeStatusDto>();

        if (!string.IsNullOrEmpty(userId))
        {
            foreach (var volume in volumes.Item1)
            {
                var status = await GetVolumeStatusInfo(volume.Id, userId, token);
                userVolumeStatuses.Add(status);
            }
        }

        return (volumes.Item1, volumes.Item2, userVolumeStatuses);
    }

    public async Task<Reading?> GetReading(Guid id, CancellationToken token = default)
    {
        var context = dbContextFactory.CreateDbContext();
        return  await context.Readings.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, token);
    }
}

public class SortDefinitions<T>
{
    public Func<T, object> SortFunction { get; set; }
    public bool Descending { get; set; }
}
