namespace MangaShelf.DAL.Interfaces;

public interface IAuditableEntity
{
    public string CreatedBy { get; set; }
    DateTimeOffset CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
}
