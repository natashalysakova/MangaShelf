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
            .Where(x => x.Series!.Title == series).SingleOrDefault();
    }

    public IQueryable<Volume> GetAllFullPaginated(IPaginationOptions? paginationOptions = default)
    {
        return GetAllFull().ApplyPagination(paginationOptions);
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

    public IQueryable<Volume> GetAllWithSeries(IPaginationOptions? paginationOptions = default)
    {
        return _context.Volumes
            .Include(v => v.Series)
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

public static class PaginationExtention
{
    public static IQueryable<Volume> ApplyPagination(this IQueryable<Volume> query, IPaginationOptions? paginationOptions)
    {
        if (paginationOptions is null)
        {
            return query;
        }

        if (paginationOptions.SortDescending)
        {
            query = query.OrderByDescending(v => EF.Property<Volume>(v, paginationOptions.SortBy ?? nameof(Volume.CreatedAt)));
        }
        else
        {
            query = query.OrderBy(v => EF.Property<Volume>(v, paginationOptions.SortBy ?? nameof(Volume.CreatedAt)));
        }


        if (paginationOptions.Search is not null)
        {
            query = query.Where(volume =>
                // Search in the Volume title
                (volume.Title != null && volume.Title.Contains(paginationOptions.Search)) ||
                // Search in Series title
                (volume.Series != null && volume.Series.Title != null && volume.Series.Title.Contains(paginationOptions.Search)) ||
                // Search in Publisher name
                (volume.Series != null && volume.Series.Publisher != null && volume.Series.Publisher.Name != null && volume.Series.Publisher.Name.Contains(paginationOptions.Search)) ||
                // Also search in Series Authors if no override authors
                (volume.Series != null && volume.Series.Authors.Any(a => a.Name != null && a.Name.Contains(paginationOptions.Search)))
            );
        }



        query = query
            .Skip(paginationOptions.Skip)
            .Take(paginationOptions.Take);


        return query;
    }

}
