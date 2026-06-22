namespace MangaShelf.Common.Models;

public class VolumeInfoRequest
{
    public required string Series { get; set; }
    public int? VolumeNumber { get; set; }
    public required string Title { get; set; }
    public required string Url { get; set; }
    public string? ISBN { get; set; }
}
