using MangaShelf.DAL.Interfaces;

namespace MangaShelf.DAL;

public class BaseEntity : IEntity, IAuditableEntity, IDeletableEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset? DeletedAt { get; set; }
    public string CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
