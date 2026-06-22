using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.DAL.Identity;

public class MangaIdentityDbContext : IdentityDbContext<MangaIdentityUser>
{
    public MangaIdentityDbContext(DbContextOptions<MangaIdentityDbContext> options) : base(options)
    {

    }
}
