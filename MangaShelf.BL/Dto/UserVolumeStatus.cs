using System.Diagnostics.CodeAnalysis;
using MangaShelf.DAL.Models;

namespace MangaShelf.BL.Dto;

[ExcludeFromCodeCoverage]
public class UserVolumeStatus
{
    public LikeStatus LikeStatus { get; set; }
    public bool IsInWishlist { get; set; }
    public string OwnersipStatus { get; set; }
    public int Rating { get; set; }
    public string ReadingStatus { get; set; }
}