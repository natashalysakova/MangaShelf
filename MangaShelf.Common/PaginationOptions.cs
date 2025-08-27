using MangaShelf.Common.Interfaces;
using System.Linq.Expressions;

namespace MangaShelf.Common;

public record PaginationOptions
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string SortColumn { get; set; } = nameof(IEntity.CreatedAt);
    public bool IsAscending { get; set; } = true;
}