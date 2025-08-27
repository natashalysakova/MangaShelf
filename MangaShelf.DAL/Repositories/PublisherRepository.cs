using MangaShelf.DAL;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.DAL.Repositories
{
    public class PublisherRepository : BaseRepository<Publisher>, IPublisherRepository
    {
        public PublisherRepository(MangaDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Publisher?> GetByName(string name)
        {
            return await _context.Publishers.FirstOrDefaultAsync(x => x.Name == name);
        }
    }
}