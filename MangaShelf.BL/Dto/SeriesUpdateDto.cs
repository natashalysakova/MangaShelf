using MangaShelf.DAL.Models;

namespace MangaShelf.BL.Dto;

public class SeriesUpdateDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? OriginalName { get; set; }
    public SeriesStatus Status { get; set; }
    public int? TotalVolumes { get; set; }
    public IList<string> Authors { get; set; } = [];
}
