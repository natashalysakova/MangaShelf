using MangaShelf.Common;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MangaShelf.DAL.Repositories
{
    public class VolumeRepository : BaseRepository<Volume>, IVolumeRepository
    {
        public VolumeRepository(ILogger<VolumeRepository> logger, MangaDbContext context) : base(context)
        {

        }

        public async Task<Volume?> FindBySeriesNameAndNumber(string series, int volumeNumber)
        {
            return await GetAllWithIncludes()
                .FirstOrDefaultAsync(x => x.Series!.Title == series && x.Number == volumeNumber);
        }

        public async Task<IEnumerable<Volume>> GetAllWithSeries(PaginationOptions? paginationOptions = null)
        {
            return await GetAllWithIncludes().ApplyPagination(paginationOptions).ToListAsync();
        }

        private IQueryable<Volume> GetAllWithIncludes()
        {
            return GetAll().Include(x => x.Series);
        }
    }

    public static class PaginationExtention
    {
        public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, PaginationOptions? paginationOptions)
        {
            if (paginationOptions is not null)
            {

                if (paginationOptions.IsAscending)
                {
                    query = query.OrderBy(x => paginationOptions.SortColumn);
                }
                else
                {
                    query = query.OrderByDescending(x => paginationOptions.SortColumn);
                }

                query = query
                    .Skip((paginationOptions.PageNumber - 1) * paginationOptions.PageSize)
                    .Take(paginationOptions.PageSize);
            }

            return query;
        }
    }
}
