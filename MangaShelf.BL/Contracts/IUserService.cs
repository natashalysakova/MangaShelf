using MangaShelf.Common.Interfaces;
using MangaShelf.BL.Dto;
using MangaShelf.DAL.Models;
using MangaShelf.DAL.Identity;

namespace MangaShelf.BL.Contracts;

public interface IUserService : IService
{
    Task<User> RegisterShelfUserAsync(MangaIdentityUser user);
    Task<string?> GetVisibleNameAsync(string identityUserId);
    
    Task<User> UpdateVisibleNameAsync(string identityUserId, string visibleName);

    Task<IReadOnlyCollection<AdminUserDto>> GetAllUsersWithRolesAsync();
    Task<IReadOnlyCollection<string>> UpdateUserRolesAsync(string identityUserId, IEnumerable<string> roles);
    Task DeleteUserAsync(string identityUserId);
    Task RestoreUserAsync(string identityUserId);

}