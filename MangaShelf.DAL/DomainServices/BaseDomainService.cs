using MangaShelf.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.DAL.DomainServices;

public class BaseDomainService<T> : IDomainService<T> where T : class, IEntity
{
    protected readonly MangaDbContext _context;

    public BaseDomainService(MangaDbContext dbContext)
    {
        _context = dbContext;
    }

    public async Task<T> Add(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<(T Entity, State State)> AddOrUpdate(T entity)
    {
        var existingEntity = await _context.Set<T>().FindAsync(entity.Id);

        var entityState = _context.Entry<T>(entity).State;

        if (entityState == EntityState.Detached)
        {
            return (await Add(entity), State.Added);
        }
        else
        {
            return (await Update(entity), State.Updated);
        }
    }

    public async Task<IEnumerable<T>> AddRange(IEnumerable<T> entities)
    {
        await _context.Set<T>().AddRangeAsync(entities);
        await _context.SaveChangesAsync();
        return entities;
    }

    public async Task<bool> Delete(T entity)
    {
        _context.Set<T>().Remove(entity);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> Delete(Guid id)
    {
        var entity = await _context.Set<T>().FindAsync(id);
        if (entity is null)
        {
            return false;
        }
        return await Delete(entity);
    }

    public async Task<T?> Get(Guid id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public IQueryable<T> GetAll(bool tracking = false)
    {
        var result = _context.Set<T>().AsQueryable<T>();
        if (!tracking)
        {
            result = result.AsNoTracking();
        }

        return result;
    }

    public async Task<T> Update(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return entity;
    }
}
