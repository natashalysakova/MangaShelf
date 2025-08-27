namespace MangaShelf.Common.Interfaces;

public interface IEntity : IAuditableEntity, IDeletableEntity
{
    Guid Id { get; set; }
}
