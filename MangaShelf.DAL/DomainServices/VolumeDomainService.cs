using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.DAL.DomainServices;

public class VolumeDomainService : BaseDomainService<Volume>, IVolumeDomainService
{
    internal VolumeDomainService(MangaDbContext context) : base(context)
    {

    }

    public Volume? FindBySeriesNameTitleAndNumber(string series, int volumeNumber, string volumeTitle)
    {
        return _context.Volumes
            .Where(x => x.Number == volumeNumber && x.Title == volumeTitle)
            .Include(x => x.Series)
            .Where(x => x.Series!.Title == series)
            .IgnoreQueryFilters()
            .SingleOrDefault();
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
