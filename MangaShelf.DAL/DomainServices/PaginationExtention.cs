using System.Linq.Expressions;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.DAL.DomainServices;

public static class PaginationExtention
{
    public static IQueryable<Volume> Filter(this IQueryable<Volume> query, IFilterOptions? paginationOptions)
    {
        if (paginationOptions is null)
        {
            return query;
        }

        if (paginationOptions?.ReleaseFilter != ReleaseFilter.None)
        {
            switch (paginationOptions!.ReleaseFilter)
            {
                case ReleaseFilter.Released:
                    query = query.Where(x => x.IsPreorder == false);
                    break;
                case ReleaseFilter.Preorder:
                    query = query.Where(x => x.IsPreorder == true);
                    break;
            }
        }

        if (!string.IsNullOrEmpty(paginationOptions?.Search))
        {
            query = query.Where(x =>
                EF.Functions.Like(x.Title, $"%{paginationOptions.Search}%") ||
                EF.Functions.Like(x.Series!.Title, $"%{paginationOptions.Search}%") ||
                EF.Functions.Like(x.Series!.Publisher!.Name, $"%{paginationOptions.Search}%") ||
                x.Series.Authors.Any(a => EF.Functions.Like(a.Name, $"%{paginationOptions.Search}%")));
        }

        Func<IQueryable<Volume>, Expression<Func<Volume, object>>, IOrderedQueryable<Volume>> orderBy = 
            paginationOptions.OrderIsAsc 
                ? Queryable.OrderBy 
                : Queryable.OrderByDescending;

        switch (paginationOptions.OrderBy)
        {
            case OrderBy.SeriesTitle:
                query = orderBy(query, x => x.Series.Title).ThenBy(x => x.Number);
                break;
            case OrderBy.ReleaseDate:
                query = orderBy(query, x => x.ReleaseDate);
                break;
            case OrderBy.Popularity:
                query = orderBy(query, x => x.Likes.Count);
                break;
            case OrderBy.Rating:
                query = orderBy(query, x => x.AvgRating);
                break;
            case OrderBy.PreorderDate:
                query = orderBy(query, x => x.PreorderStart);
                break;
        }

        return query;
    }
    public static IQueryable<Volume> ApplyPagination(this IQueryable<Volume> query, IFilterOptions? paginationOptions)
    {
        if (paginationOptions is null)
        {
            return query;
        }
        
        if (paginationOptions?.Take != 0)
        {
            query = query.Skip(paginationOptions.Skip).Take(paginationOptions.Take);
        }

        return query;
    }

    public static async Task<int> GetTotalPagesAsync(this IQueryable<Volume> query, IFilterOptions? paginationOptions)
    {
        if (paginationOptions is null || paginationOptions.Take == 0)
        {
            return 1;
        }

        var totalCount = await query.CountAsync();
        var pagesCount = (double)totalCount / paginationOptions!.Take;
        var totalPages = (int)Math.Ceiling(pagesCount);

        return totalPages;
    }

}
