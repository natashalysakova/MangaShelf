using MangaShelf.DAL;
using MangaShelf.DAL.Models;
using MangaShelf.DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.Infrastructure.Installer
{
    public class AuthorRepository : BaseRepository<Author>, IAuthorRepository
    {
        public AuthorRepository(MangaDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Author?> GetByName(string name)
        {
            return await _context.Authors.FirstOrDefaultAsync(x=>x.Name == name);
        }
    }
}