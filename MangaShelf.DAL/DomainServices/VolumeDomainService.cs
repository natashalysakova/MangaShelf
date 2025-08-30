using MangaShelf.Common;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MangaShelf.DAL.DomainServices;

public class VolumeDomainService : BaseDomainService<Volume>, IVolumeDomainService
{
    public VolumeDomainService(ILogger<VolumeDomainService> logger, MangaDbContext context) : base(context)
    {

    }

    public async Task<Volume?> FindBySeriesNameTitleAndNumber(string series, int volumeNumber, string volumeTitle)
    {
        return await _context.Volumes.Where(x => x.Number == volumeNumber && x.Title == volumeTitle).Include(x => x.Series).Where(x => x.Series!.Title == series).SingleOrDefaultAsync();
    }

    public async Task<(IEnumerable<Volume>, int)> GetAllWithSeries(PaginationOptions? paginationOptions = null)
    {
        var totalCount = await _context.Volumes.CountAsync();
        var query = GetAllWithIncludes(false);
        var totalPages = (int)Math.Floor(totalCount / (double)(paginationOptions?.PageSize ?? totalCount));
        return (await query.ApplyPagination(paginationOptions).ToListAsync(), totalPages);
    }

    public async Task<IEnumerable<Volume>> GetLatestPreorders(int count)
    {
        return await GetAllWithIncludes()
            .Where(v => v.IsPreorder)
            .OrderByDescending(v => v.PreorderStart)
            .Take(count).ToListAsync();
    }

    public async Task<IEnumerable<Volume>> GetNewestReleases(int count)
    {
        return await GetAllWithIncludes()
            .Where(v => !v.IsPreorder)
            .OrderByDescending(v => v.ReleaseDate)
            .Take(count).ToListAsync();
    }

    private IQueryable<Volume> GetAllWithIncludes(bool tracking = true)
    {
        return GetAll(tracking).Include(x => x.Series);
    }
}

public static class PaginationExtention
{
    public static IQueryable<Volume> ApplyPagination(this IQueryable<Volume> query, PaginationOptions? paginationOptions)
    {

        query = query.OrderByDescending(v => v.PreorderStart == null ? v.CreatedAt : v.PreorderStart);

        if (paginationOptions is not null)
        {
            var skip = paginationOptions.PageNumber * paginationOptions.PageSize;
            query = query
                .Skip(skip)
                .Take(paginationOptions.PageSize);

        }

        return query;
    }
}
