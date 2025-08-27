using MangaShelf.DAL;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.DAL.Repositories
{
    public class AuthorRepository : BaseRepository<Author>, IAuthorRepository
    {
        public AuthorRepository(MangaDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Author?> GetByName(string name)
        {
            return await _context.Authors.FirstOrDefaultAsync(x => x.Name == name);
        }
    }
}