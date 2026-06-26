namespace MangaShelf.Common.Interfaces;

public interface IImageManager
{
    Task<string?> CreateSmallImage(string coverImageUrl);
    Task<string?> DownloadFileFromWeb(string url, string publicId);
    Task<string?> SaveFlagFromCDN(string countryCode);
    Task<string?> CropImage(string coverImageUrl);
    Task<string?> CropImage(string coverImageUrl, CropZone cropZone);
    (int width, int height) GetImageDimensions(string coverImageUrl);
    Task<string?> UploadImage(Stream imageStream, string publicId, string originalFileName);
}
