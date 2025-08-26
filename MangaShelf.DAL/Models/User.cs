using MangaShelf.DAL.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace MangaShelf.DAL.Models;

public class User : BaseEntity
{
    public required string IdentityUserId { get; set; }
    public string? VisibleUsername { get; set; }
    public virtual ICollection<Ownership> OwnedVolumes { get; set; } = new List<Ownership>();
    public virtual ICollection<Reading> Readings { get; set; } = new List<Reading>();
}
