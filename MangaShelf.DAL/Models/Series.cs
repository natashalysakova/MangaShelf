namespace MangaShelf.DAL.Models;

public class Series : BaseEntity
{
    public required string Title { get; set; }
    public string? OriginalName { get; set; }
    public ICollection<string> Aliases { get; set; } = new List<string>();
    public int MalId { get; set; }

    public bool Ongoing { get; set; }
    public int? TotalVolumes { get; set; }
    
    public Guid PublisherId { get; set; }
    public virtual Publisher? Publisher { get; set; }

    public virtual ICollection<Author> Authors { get; set; } = new List<Author>();
    public virtual ICollection<Volume> Volumes { get; set; } = new List<Volume>();
}
