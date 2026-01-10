using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.DAL.DomainServices;

public abstract class BaseDomainService<T> : BaseDomainService<MangaDbContext, T> where T : class, IEntity
{
    protected BaseDomainService(MangaDbContext context) : base(context)
    {
    }
}

public abstract class BaseDomainService<C, T> : IDomainService<T> where T : class, IEntity where C : DbContext
{
    protected C _context;

    public BaseDomainService(C context)
    {
        _context = context;
    }

    public async Task<T> Add(T entity, bool shouldSave = false, CancellationToken token = default)
    {
        await _context.Set<T>().AddAsync(entity, token);
        await Save(shouldSave, token);
        return entity;
    }

    public async Task<(T Entity, State State)> AddOrUpdate(T entity, bool shouldSave = false, CancellationToken token = default)
    {
        var existingEntity = await _context.Set<T>().FindAsync(entity.Id, token);

        var entityState = _context.Entry<T>(entity).State;

        if (entityState == EntityState.Detached)
        {
            return (await Add(entity, shouldSave, token), State.Added);
        }
        else
        {
            return (await Update(entity, shouldSave, token), State.Updated);
        }
    }

    //public async Task<IEnumerable<T>> AddRange(IEnumerable<T> entities, CancellationToken token = default)
    //{
    //    await _context.Set<T>().AddRangeAsync(entities, token);
    //    await _context.SaveChangesAsync(token);
    //    return entities;
    //}

    //public async Task<bool> Delete(T entity, CancellationToken token = default)
    //{
    //    _context.Set<T>().Remove(entity);
    //    var result = await _context.SaveChangesAsync(token);
    //    return result > 0;
    //}

    //public async Task<bool> Delete(Guid id, CancellationToken token = default)
    //{
    //    var entity = await _context.Set<T>().FindAsync(id, token);
    //    if (entity is null)
    //    {
    //        return false;
    //    }
    //    return await Delete(entity, token);
    //}

    //public async Task<T?> Get(Guid id, CancellationToken token = default)
    //{
    //    return await _context.Set<T>().FindAsync(id, token);
    //}

    //public async Task<IEnumerable<T>> GetAll(bool tracking = false, CancellationToken token = default)
    //{
    //    var result = _context.Set<T>().AsQueryable<T>();
    //    if (!tracking)
    //    {
    //        result = result.AsNoTracking();
    //    }

    //    return await result.ToListAsync(token);
    //}

    public async Task<T> Update(T entity, bool shouldSave = false, CancellationToken token = default)
    {
        _context.Entry(entity).State = EntityState.Modified;
        
        await Save(shouldSave, token);
        return entity;
    }

    private async Task<int> Save(bool shouldSave, CancellationToken token = default)
    {
        if (shouldSave)
        {
            return await _context.SaveChangesAsync(token);
        }
        return -1;
    }
}
