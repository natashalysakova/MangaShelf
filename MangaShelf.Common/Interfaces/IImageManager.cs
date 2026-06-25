namespace MangaShelf.Common.Interfaces;

public interface IImageManager
{
    string CreateSmallImage(string coverImageUrl);
    Task<string?> DownloadFileFromWeb(string url, string publicId);
    string SaveFlagFromCDN(string countryCode);
    string CropImage(string coverImageUrl);
    string? CropImage(string coverImageUrl, int left, int top, int right, int bottom);
    (int width, int height) GetImageDimensions(string coverImageUrl);
}
