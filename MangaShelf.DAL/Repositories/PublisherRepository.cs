using MangaShelf.DAL;
using MangaShelf.DAL.Models;
using MangaShelf.DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.Infrastructure.Installer
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