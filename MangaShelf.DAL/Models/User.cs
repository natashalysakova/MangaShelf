using Microsoft.AspNetCore.Identity;

namespace MangaShelf.DAL.Models
{
    public class User : IdentityUser
    {
        public string VisibleUsername { get; set; }
        public virtual ICollection<Ownership> OwnedVolumes { get; set; } = new List<Ownership>();
        public virtual ICollection<Reading> Readings { get; set; } = new List<Reading>();
    }
}
