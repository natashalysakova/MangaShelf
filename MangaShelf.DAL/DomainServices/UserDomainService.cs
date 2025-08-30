using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;

namespace MangaShelf.DAL.DomainServices;

public class UserDomainService : BaseDomainService<User>, IUserDomainService
{
    public UserDomainService(MangaDbContext dbContext) : base(dbContext)
    {
    }
}