using MangaShelf.Common.Interfaces;

namespace MangaShelf.Common;

public record PaginationOptions : IPaginationOptions
{
    public required int PageNumber { get; init; }
    public required int PageSize { get; init; }
    public int Skip
    {
        get
        {
            return PageNumber * PageSize;
        }
    }
    public int Take { get => PageSize; }
    public string Search { get; init; }
    public string SortBy { get ; init; }
    public bool SortDescending { get ; init; }
}
