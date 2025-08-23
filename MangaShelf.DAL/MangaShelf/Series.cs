namespace MangaShelf.DAL.MangaShelf
{
    public class Series : BaseEntity
    {
        public required string Title { get; set; }

        public Guid PublisherId { get; set; }
        public virtual Publisher? Publisher { get; set; }
        public virtual ICollection<Volume> Volumes { get; set; } = new List<Volume>();
    }
}
