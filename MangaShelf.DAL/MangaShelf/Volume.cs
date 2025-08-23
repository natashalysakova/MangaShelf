namespace MangaShelf.DAL.MangaShelf
{
    public class Volume : BaseEntity
    {
        public required string Title { get; set; }
        public required Series Series { get; set; }
        public int VolumeNumber { get; set; }
        public string? CoverImageUrl { get; set; }
        public DateTimeOffset? ReleaseDate { get; set; }
    }
}
