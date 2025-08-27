namespace MangaShelf.Common.Interfaces;

public interface IDeletableEntity
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
}
