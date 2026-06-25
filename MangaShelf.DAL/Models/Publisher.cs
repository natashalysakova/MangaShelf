namespace MangaShelf.DAL.Models;

public class Publisher : BaseEntity
{
    public required string Name { get; set; }
    public List<string> AlternativeNames { get; set; } = new List<string>();

    public string? Url { get; set; }

    public Guid CountryId { get; set; }
    public virtual Country? Country { get; set; }

    public virtual ICollection<Series> Mangas { get; set; } = new List<Series>();
}
