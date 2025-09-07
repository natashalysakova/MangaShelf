namespace MangaShelf.Common.Interfaces;

public interface IShelfDomainService
{
}

public interface IDomainService<T> 
{
    Task<T> Add(T entity, bool shouldSave = false, CancellationToken token = default);
    Task<T> Update(T entity, bool shouldSave = false, CancellationToken token = default);
    Task<(T Entity, State State)> AddOrUpdate(T entity, bool shouldSave = false, CancellationToken token = default);
    
    //Task<IEnumerable<T>> AddRange(IEnumerable<T> entities, CancellationToken token = default);
    //Task<bool> Delete(T entity, CancellationToken token = default);
    //Task<bool> Delete(Guid id, CancellationToken token = default);
    //Task<T?> Get(Guid id, CancellationToken token = default);
    //Task<IEnumerable<T>> GetAll(bool tracking = false, CancellationToken token = default);
}

public enum State
{
    None,
    Added,
    Updated,
    Deleted,
    Unchanged,
}