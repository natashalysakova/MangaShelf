namespace MangaShelf.Common.Interfaces;

public interface IPaginationOptions
{
    int Skip { get; }
    int Take { get; }

    string Search { get; init; }
    string SortBy { get; init; }
    bool SortDescending { get; init; }
}