namespace MangaShelf.DAL.Interfaces;

public interface IDomainService<T>
{
    Task<T> Add(T entity);
    Task<IEnumerable<T>> AddRange(IEnumerable<T> entities);
    Task<T> Update(T entity);
    Task<(T Entity, State State)> AddOrUpdate(T entity);
    Task<bool> Delete(T entity);
    Task<bool> Delete(Guid id);
    Task<T?> Get(Guid id);
    IQueryable<T> GetAll(bool tracking = false);
}

public enum State
{
    Added,
    Updated,
    Deleted,
    Unchanged
}