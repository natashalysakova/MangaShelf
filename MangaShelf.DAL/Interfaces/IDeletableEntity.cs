namespace MangaShelf.DAL.Interfaces
{
    public interface IDeletableEntity
    {
        bool IsDeleted { get; set; }
        DateTimeOffset? DeletedAt { get; set; }
    }
}
