namespace MangaShelf.DAL.MangaShelf.Models
{
    public class Country : BaseEntity
    {
        public required string Name { get; set; }
        public required string CountryCode { get; set; }
        public required string FlagUrl { get; set; }

        public virtual ICollection<Publisher> Publishers { get; set; } = new List<Publisher>();
    }
}
