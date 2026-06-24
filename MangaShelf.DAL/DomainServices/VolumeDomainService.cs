using MangaShelf.Common.Interfaces;
using MangaShelf.Common.Models;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MangaShelf.DAL.DomainServices;

public class VolumeDomainService : BaseDomainService<Volume>, IVolumeDomainService
{
    internal VolumeDomainService(MangaDbContext context) : base(context)
    {

    }

    public Volume? FindVolumeFromParsedInfo(VolumeInfoRequest volumeInfo)
    {
        var query = _context.Volumes
            .Include(x => x.Series)
            .IgnoreQueryFilters();

        if (!string.IsNullOrWhiteSpace(volumeInfo.Url))
        {
            var volumeByUrl = FindSingleMatchOrDefault(query, x => x.PurchaseUrl == volumeInfo.Url);
            if (volumeByUrl != null)
            {
                return volumeByUrl;
            }
        }

        if (!string.IsNullOrWhiteSpace(volumeInfo.ISBN))
        {
            var volumeByIsbn = FindSingleMatchOrDefault(query, x => x.ISBN == volumeInfo.ISBN);
            if (volumeByIsbn != null)
            {
                return volumeByIsbn;
            }
        }

        return FindSingleMatchOrDefault(query, x =>
            x.Series!.Title == volumeInfo.Series &&
            x.Number == volumeInfo.VolumeNumber &&
            x.Title == volumeInfo.Title);
    }

    private static Volume? FindSingleMatchOrDefault(IQueryable<Volume> query, Expression<Func<Volume, bool>> predicate)
    {
        var matches = query
            .Where(predicate)
            .Take(2)
            .ToList();

        return matches.Count == 1 ? matches[0] : null;
    }

    public IQueryable<Volume> GetAllFullPaginated(IFilterOptions? paginationOptions = default)
    {
        return GetAllFull()
            .Filter(paginationOptions)
            .ApplyPagination(paginationOptions);
    }

    public IQueryable<Volume> GetAllFull()
    {
        var query = _context.Volumes
            .Include(v => v.Series)
                .ThenInclude(s => s.Publisher)
                    .ThenInclude(p => p.Country)
            .Include(v => v.Series)
                .ThenInclude(x => x.Authors);


        return query;
    }

    public IQueryable<Volume> GetAllWithSeries(IFilterOptions? paginationOptions = default)
    {
        return _context.Volumes
            .Include(v => v.Series)
            .Filter(paginationOptions)
            .ApplyPagination(paginationOptions);
    }


    public Volume? GetFullVolume(Guid id)
    {
        return GetAllFull()
            .FirstOrDefault(v => v.Id == id);
    }

    public IQueryable<Volume> GetLatestPreorders(int count)
    {
        return _context.Volumes
            .Include(v => v.Series)
            .Where(v => v.IsPreorder && v.IsPublishedOnSite)
            .OrderByDescending(v => v.PreorderStart)
            .Take(count);
    }

    public IQueryable<Volume> GetNewestReleases(int count)
    {
        return _context.Volumes
            .Include(v => v.Series)
            .Where(v => !v.IsPreorder && v.IsPublishedOnSite)
            .OrderByDescending(v => v.ReleaseDate)
            .Take(count);
    }
}
