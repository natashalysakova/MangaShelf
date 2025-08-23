namespace MangaShelf.DAL.Interfaces
{
    public interface IEntity : IAuditableEntity, IDeletableEntity
    {
        Guid Id { get; set; }
    }
}
