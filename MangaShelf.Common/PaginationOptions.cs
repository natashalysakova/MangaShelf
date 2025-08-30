namespace MangaShelf.Common;

public record PaginationOptions
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortColumn { get; set; }
    public bool IsAscending { get; set; } = true;
}