using MangaShelf.DAL.Interfaces;

namespace MangaShelf.DAL.System.Models;

public class Parser : IEntity
{
    public Guid Id { get; set; }
    public string ParserName { get; set; } = null!;
    public DateTimeOffset NextRun { get; set; }
    public ParserStatus Status { get; set; }
    public virtual ICollection<ParserJob> Jobs { get; set; } = new List<ParserJob>();
}
