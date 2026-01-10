namespace MangaShelf.DAL.Models;

public class Likes : BaseEntity
{
    public Guid UserId { get; set; }
    public virtual User? User { get; set; }
    public Guid VolumeId { get; set; }
    public virtual Volume? Volume { get; set; }
    public LikeStatus Status { get; set; }
}
