namespace MangaShelf.Common.Interfaces;

public interface IRepository<T>
{
    Task<T> Add(T entity);
    Task<T> Update(T entity);
    Task<bool> Delete(T entity);
    Task<bool> Delete(Guid id);
    Task<T?> Get(Guid id);
    IQueryable<T> GetAll(bool tracking = false);
}
