using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;
using MangaShelf.DAL.Identity;

namespace MangaShelf.BL.Contracts;

public interface IUserService : IService
{
    Task<User> RegisterShelfUserAsync(MangaIdentityUser user);
    Task<string?> GetVisibleNameAsync(string identityUserId);
    
    Task<User> UpdateVisibleNameAsync(string identityUserId, string visibleName);

}