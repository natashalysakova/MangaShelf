namespace MangaShelf.DAL.Models;

public class Author : BaseEntity
{
    public required string Name { get; set; }
    public virtual ICollection<Series> Series { get; set; } = new List<Series>();
    public virtual ICollection<Volume> Volumes { get; set; } = new List<Volume>();

    override public string ToString() => Name;
}