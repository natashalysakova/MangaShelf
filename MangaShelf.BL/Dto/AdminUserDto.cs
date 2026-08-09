using System.Diagnostics.CodeAnalysis;

namespace MangaShelf.BL.Dto;

[ExcludeFromCodeCoverage]
public class AdminUserDto
{
    public required string IdentityUserId { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? VisibleName { get; set; }
    public bool IsDeleted { get; set; }
    public List<string> Roles { get; set; } = [];
}
