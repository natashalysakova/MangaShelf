namespace MangaShelf.DAL.Models
{
    public class Ownership : BaseEntity
    {
        public Guid UserId { get; set; }
        public virtual User? User { get; set; }
        public Guid VolumeId { get; set; }
        public virtual Volume? Volume { get; set; }
        public VolumeStatus Status { get; set; }
        public VolumeType Type { get; set; }
        public DateTimeOffset Date { get; set; }

        public enum VolumeStatus
        {
            Wishlist = 0,
            Preorder = 1,
            Own = 2,
            Gone = 3
        }

        public enum VolumeType
        {
            Physical = 0,
            Digital = 1
        }
    }
}
