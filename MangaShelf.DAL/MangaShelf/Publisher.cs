namespace MangaShelf.DAL.MangaShelf
{
    public class Publisher : BaseEntity
    {
        public required string Name { get; set; }
        public string? Url { get; set; }

        public Guid CountryId { get; set; }
        public virtual Country? Country { get; set; }

        public virtual ICollection<Series> Mangas { get; set; } = new List<Series>();
    }
}
