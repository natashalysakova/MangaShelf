using ImageMagick;
using Microsoft.Extensions.Logging;

namespace MangaShelf.Common.Interfaces;

public class ImageManager : IImageManager
{
    private const string serverRoot = "wwwroot";
    const string imageDir = "images";

    private readonly ILogger<ImageManager> _logger;

    public ImageManager(ILogger<ImageManager> logger)
    {
        _logger = logger;
    }

    public async Task<string?> CropImage(string imageUrl)
    {
        var sourceImage = Path.Combine(serverRoot, imageUrl);

        if (!File.Exists(sourceImage))
            return null;

        try
        {
            using var image = new MagickImage(sourceImage);

            var width = (int)image.Width;
            var height = (int)image.Height;

            using var pixels = image.GetPixels();

            var maxValue = Quantum.Max;

            var hasTransparent = HasTransparency(image);

            // Helper method to check if a pixel should be considered as border
            bool IsEmptyPixel(IMagickColor<ushort>? color)
            {
                if (color == null)
                    return true;

                // Check if nearly transparent (alpha < 50%)
                if (color.A < maxValue * 0.99)
                    return true;

                if (!hasTransparent)
                {
                    // Check if it's white or almost white (all channels > 95%)
                    var whiteThreshold = maxValue * 0.50;
                    if (color.R > whiteThreshold && color.G > whiteThreshold && color.B > whiteThreshold)
                        return true;
                }

                return false;
            }

            // Find top boundary
            int top = 0;
            for (int y = 0; y < height; y++)
            {
                bool hasContentPixel = false;
                for (int x = 0; x < width; x++)
                {
                    var pixel = pixels[x, y];
                    var color = pixel?.ToColor();

                    if (!IsEmptyPixel(color))
                    {
                        hasContentPixel = true;
                        break;
                    }
                }

                if (hasContentPixel)
                {
                    top = y;
                    break;
                }
            }

            // Find bottom boundary
            int bottom = height - 1;
            for (int y = height - 1; y >= 0; y--)
            {
                bool hasContentPixel = false;
                for (int x = 0; x < width; x++)
                {
                    var pixel = pixels[x, y];
                    var color = pixel?.ToColor();

                    if (!IsEmptyPixel(color))
                    {
                        hasContentPixel = true;
                        break;
                    }
                }

                if (hasContentPixel)
                {
                    bottom = y;
                    break;
                }
            }

            // Find right boundary - check only top 85% of the image
            int right = width - 1;
            var checkHeightForRight = (int)(height * 0.9);

            for (int x = width - 1; x >= 0; x--)
            {
                bool hasContentPixel = false;
                // Only scan the top 85% of the image height
                for (int y = 0; y < checkHeightForRight; y++)
                {
                    var pixel = pixels[x, y];
                    var color = pixel?.ToColor();

                    if (!IsEmptyPixel(color))
                    {
                        hasContentPixel = true;
                        break;
                    }
                }

                if (hasContentPixel)
                {
                    right = x;
                    break;
                }
            }

            // Find left boundary
            int left = 0;
            var checkHeightForleft = (int)(height * 0.85);
            for (int x = 0; x < width; x++)
            {
                bool hasContentPixel = false;
                for (int y = 0; y < checkHeightForleft; y++)
                {
                    var pixel = pixels[x, y];
                    var color = pixel?.ToColor();

                    if (!IsEmptyPixel(color))
                    {
                        hasContentPixel = true;
                        break;
                    }
                }

                if (hasContentPixel)
                {
                    left = x;
                    break;
                }
            }

            return await CropImage(imageUrl, new CropZone(left, top, right, bottom));
        }
        catch (Exception ex)
        {
            _logger.LogError("Error cropping image: " + ex.Message);
        }

        return null;
    }

    public async Task<string?> CropImage(string imageUrl, CropZone cropZone)
    {
        var sourceImage = Path.Combine(serverRoot, imageUrl);

        if (!File.Exists(sourceImage))
            return null;

        try
        {
            using var image = new MagickImage(sourceImage);

            var left = cropZone.Left;
            var top = cropZone.Top;
            var right = cropZone.Right;
            var bottom = cropZone.Bottom;

            var width = (int)image.Width;
            var height = (int)image.Height;

            // Calculate crop dimensions
            var cropWidth = right - left + 1;
            var cropHeight = bottom - top + 1;

            // Only crop if we found valid boundaries
            if (cropWidth > 0 && cropHeight > 0 && (left > 0 || top > 0 || right < width - 1 || bottom < height - 1))
            {
                var cropGeometry = new MagickGeometry(left, top, (uint)cropWidth, (uint)cropHeight);
                image.Crop(cropGeometry);
                image.ResetPage();

                // Overwrite the original image
                var fileInfo = new FileInfo(sourceImage);
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.Name);
                var extension = fileInfo.Extension;
                var fileName = $"{fileNameWithoutExtension}_crop{extension}";
                var croppedImagePath = Path.Combine(Path.GetDirectoryName(sourceImage)!, fileName);
                await image.WriteAsync(croppedImagePath);

                var pathToReturn = Path.Combine(Path.GetDirectoryName(imageUrl)!, fileName);

                return pathToReturn;

            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error cropping image: " + ex.Message);
        }

        return null;

    }

    public async Task<string?> CreateSmallImage(string coverImageUrl)
    {
        var sourceImage = Path.Combine(serverRoot, coverImageUrl);

        if (Path.Exists(sourceImage))
        {
            // Create destination path
            var fileInfo = new FileInfo(sourceImage);
            var filename = fileInfo.Name;
            var destiantionFolder = Path.Combine(imageDir, "small");
            var destinationPath = Path.Combine(serverRoot, destiantionFolder, filename);
            var destinationDirectory = Path.GetDirectoryName(destinationPath);

            if (!Directory.Exists(destinationDirectory))
                Directory.CreateDirectory(destinationDirectory);

            // Resize image to 360px height
            using var image = new MagickImage(sourceImage);

            var size = new MagickGeometry(0, 360);

            image.Resize(size, FilterType.Lanczos);
            image.UnsharpMask(2, 1);

            // Save the result
            image.Write(destinationPath);

            // Return relative path
            return Path.Combine(destiantionFolder, filename);
        }

        return coverImageUrl; // Return original if resize fails
    }

    public async Task<string?> DownloadFileFromWeb(string url, string publicId)
    {
        if (url.Contains('?'))
        {
            var indexOfQuestionMark = url.IndexOf('?');
            url = url.Substring(0, indexOfQuestionMark).Trim('?');
        }

        var extention = new FileInfo(url).Extension;
        var destiantionFolder = Path.Combine(imageDir, "series", publicId);
        var filename = $"{Guid.NewGuid()}{extention}";

        try
        {
            using (var client = new HttpClient())
            {
                using (var response = await client.GetAsync(url))
                {
                    byte[] imageBytes =
                        await response.Content.ReadAsByteArrayAsync();

                    var localDirectory = Path.Combine(serverRoot, destiantionFolder);
                    var localPath = Path.Combine(localDirectory, filename);

                    if (!Directory.Exists(localDirectory))
                        Directory.CreateDirectory(localDirectory);

                    await File.WriteAllBytesAsync(localPath, imageBytes);
                    _logger.LogInformation($"Downloaded image from {url} to {localPath}");
                }
            }

            var urlPath = Path.Combine(destiantionFolder, filename);

            return urlPath;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<string?> SaveFlagFromCDN(string countryCode)
    {
        var urls = new List<string> {
        $"https://flagcdn.com/40x30/{countryCode}.webp" };

        var destiantionFolder = Path.Combine(imageDir, "countries");
        var localDirectory = Path.Combine(serverRoot, destiantionFolder);


        foreach (var url in urls)
        {
            var extention = Path.GetExtension(url);
            var filename = $"{countryCode}{extention}";

            using (var client = new HttpClient())
            {
                using (var response = await client.GetAsync(url))
                {
                    byte[] imageBytes =
                        await response.Content.ReadAsByteArrayAsync();

                    var localPath = Path.Combine(localDirectory, filename);

                    if (!Directory.Exists(localDirectory))
                        Directory.CreateDirectory(localDirectory);

                    await File.WriteAllBytesAsync(localPath, imageBytes);
                }
            }
        }

        return Path.Combine(destiantionFolder, $"{countryCode}.webp");
    }

    public (int width, int height) GetImageDimensions(string coverImageUrl)
    {
        var sourceImage = Path.Combine(serverRoot, coverImageUrl);

        if (!File.Exists(sourceImage))
            throw new FileNotFoundException("Image not found", sourceImage);

        using (var image = new MagickImage(sourceImage))
        {
            return ((int)image.Width, (int)image.Height);
        }
    }

    public async Task<string?> UploadImage(Stream imageStream, string publicId, string originalFileName)
    {
        try
        {
            var destinationFolder = Path.Combine(imageDir, "series", publicId);
            var extension = Path.GetExtension(originalFileName);
            var filename = $"{Guid.NewGuid()}{extension}";
            var localDirectory = Path.Combine(serverRoot, destinationFolder);
            var localPath = Path.Combine(localDirectory, filename);

            if (!Directory.Exists(localDirectory))
                Directory.CreateDirectory(localDirectory);

            using var memoryStream = new MemoryStream();
            await imageStream.CopyToAsync(memoryStream);
            await File.WriteAllBytesAsync(localPath, memoryStream.ToArray());

            _logger.LogInformation($"Uploaded image to {localPath}");

            var imagePath = Path.Combine(destinationFolder, filename);

            return imagePath;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private bool HasTransparency(MagickImage image)
    {
        // Check if image has an alpha channel
        if (!image.HasAlpha)
            return false;

        var maxValue = Quantum.Max;
        using var pixels = image.GetPixels();
        
        var width = (int)image.Width;
        var height = (int)image.Height;

        // Sample strategy: check every 10th pixel for performance
        // Adjust sampling rate based on your needs
        for (int y = 0; y < height; y += 10)
        {
            for (int x = 0; x < width; x += 10)
            {
                var pixel = pixels[x, y];
                var color = pixel?.ToColor();
                
                if (color != null && color.A < maxValue)
                {
                    return true; // Found a transparent pixel
                }
            }
        }

        return false;
    }
}

public class ImageResult
{
    public required string OriginalImage { get; set; }
    public required string CroppedImage { get; set; }
    public required string SmallImage { get; set; }
}

public interface IImageFlow
{
    Task<ImageResult> DownloadAndProcessImage(string url, string seriesPublicId);
    Task<ImageResult> UploadAndProcessImage(Stream localFileStream, string seriesPublicId, string fileName);
    Task<ImageResult> ProcessCrop(string url, CropZone? cropZone = null);
}

public struct CropZone(int Left, int Top, int Right, int Bottom)
{
    public int Left { get; } = Left;
    public int Top { get; } = Top;
    public int Right { get; } = Right;
    public int Bottom { get; } = Bottom;
}

public class ImageFlow : IImageFlow
{
    private readonly IImageManager _imageManager;
    private readonly ILogger<ImageFlow> _logger;
    public ImageFlow(IImageManager imageManager, ILogger<ImageFlow> logger)
    {
        _imageManager = imageManager;
        _logger = logger;
    }
    
    public async Task<ImageResult> DownloadAndProcessImage(string url, string seriesPublicId)
    {
        try
        {
            var originalImagePath = await _imageManager.DownloadFileFromWeb(url, seriesPublicId);
            var croppedImagePath = await _imageManager.CropImage(originalImagePath);
            var smallImagePath = await _imageManager.CreateSmallImage(croppedImagePath ?? originalImagePath);

            return new ImageResult
            {
                OriginalImage = originalImagePath,
                CroppedImage = croppedImagePath ?? originalImagePath,
                SmallImage = smallImagePath ?? originalImagePath
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading and processing image from URL: {Url}", url);
            throw;
        }        
    }

    public async Task<ImageResult> ProcessCrop(string url, CropZone? cropZone = null)
    {
        try
        {
            var croppedImagePath = cropZone.HasValue ? 
                await _imageManager.CropImage(url, cropZone.Value) : 
                await _imageManager.CropImage(url);

            var smallImagePath = await _imageManager.CreateSmallImage(croppedImagePath ?? url);

            return new ImageResult
            {
                OriginalImage = url,
                CroppedImage = croppedImagePath ?? url,
                SmallImage = smallImagePath ?? url
            };

        }
        catch (Exception)
        {
            _logger.LogError("Error uploading and processing image from local path: {LocalPath}", url);
            throw;
        }
    }

    public async Task<ImageResult> UploadAndProcessImage(Stream localFileStream, string seriesPublicId, string fileName)
    {
        try
        {
            var originalImagePath = await _imageManager.UploadImage(localFileStream, seriesPublicId, fileName);
            var croppedImagePath = await _imageManager.CropImage(originalImagePath);
            var smallImagePath = await _imageManager.CreateSmallImage(croppedImagePath ?? originalImagePath);

            return new ImageResult
            {
                OriginalImage = originalImagePath,
                CroppedImage = croppedImagePath ?? originalImagePath,
                SmallImage = smallImagePath ?? originalImagePath
            };

        }
        catch (Exception)
        {
            _logger.LogError("Error uploading and processing image from local stream: {SeriesPublicId}, {FileName}", seriesPublicId, fileName);
            throw;
        }
    }
}
