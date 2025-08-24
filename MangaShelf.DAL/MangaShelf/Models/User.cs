namespace MangaShelf.DAL.MangaShelf.Models
{
    public class User : BaseEntity
    {
        public string ApplicationUserId { get; set; }
        public virtual ICollection<Ownership> Volumes { get; set; } = new List<Ownership>();
        public virtual ICollection<Reading> Readings { get; set; } = new List<Reading>();
    }
}
