using MangaShelf.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.DAL.Repositories;

public class BaseRepository<T> : IRepository<T> where T: class
{
    protected readonly MangaDbContext _context;

    public BaseRepository(MangaDbContext dbContext)
    {
        _context = dbContext;
    }

    public async Task<T> Add(T entity)
    {
        _context.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> Delete(T entity)
    {
        _context.Remove(entity);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> Delete(Guid id)
    {
        var entity = await _context.Set<T>().FindAsync(id);
        if(entity is null)
        {
            return false;
        }
        return await Delete(entity);
    }

    public async Task<T?> Get(Guid id)
    {
        return await _context.Set<T>().FindAsync(id);  
    }

    public async Task<IEnumerable<T>> GetAll()
    {
        return await _context.Set<T>().AsNoTracking().ToListAsync();
    }

    public async Task<T> Update(T entity)
    {
        _context.Entry<T>(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return entity;
    }
}
