using ImageMagick;

namespace MangaShelf.Common.Interfaces;

public interface IImageManager
{
    string CreateSmallImage(string coverImageUrl);
    string? DownloadFileFromWeb(string url);
    string SaveFlagFromCDN(string countryCode);
    void CropImage(string coverImageUrl);
}

public class ImageManager : IImageManager
{
    private const string serverRoot = "wwwroot";
    const string imageDir = "images";

    public bool CheckIfCoverNeedAdjutment(string coverImageUrl)
    {
        var sourceImage = Path.Combine(serverRoot, coverImageUrl);

        if (!File.Exists(sourceImage))
            return false;

        try
        {
            using var image = new MagickImage(sourceImage);
            
            var width = image.Width;
            var height = image.Height;
            
            // Check left edge pixels (first 10% of width)
            var checkWidth = Math.Max(1, (int)(width * 0.10));
            
            using var pixels = image.GetPixels();

            var maxValue = Quantum.Max; // Typically 65535 for Q16
            var alphaThreshold = maxValue * 0.1;
            var whiteThreshold = maxValue * 0.95;

            for (int x = 0; x < checkWidth; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var pixel = pixels[x, y];
                    var color = pixel.ToColor();

                    // Check if pixel is colorful (not white and not transparent)
                    if (color != null &&
                        color.A >= alphaThreshold && // Not nearly transparent
                        !(color.R > whiteThreshold && color.G > whiteThreshold && color.B > whiteThreshold)) // Not nearly white
                    {
                        return false; // Found a colorful pixel, no adjustment needed
                    }
                }
            }
            
            // No colorful pixels found in the first 5% of the image
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void CropImage(string coverImageUrl)
    {
        var sourceImage = Path.Combine(serverRoot, coverImageUrl);

        if (!File.Exists(sourceImage))
            return;

        try
        {
            using var image = new MagickImage(sourceImage);
            
            var width = (int)image.Width;
            var height = (int)image.Height;
            
            using var pixels = image.GetPixels();
            
            var maxValue = Quantum.Max;
            
            // Helper method to check if a pixel should be considered as border
            bool IsEmptyPixel(IMagickColor<ushort>? color)
            {
                if (color == null)
                    return true;
                
                // Check if nearly transparent (alpha < 50%)
                if (color.A < maxValue * 0.99)
                    return true;
                
                // Check if it's white or almost white (all channels > 95%)
                var whiteThreshold = maxValue * 0.99;
                if (color.R > whiteThreshold && color.G > whiteThreshold && color.B > whiteThreshold)
                    return true;
                
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
            var checkHeightForRight = (int)(height * 0.85);
            
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
                    if(Math.Abs(right - x) > (x * 0.35)) // wide left and right fields
                    {
                        
                        if (x > (width * 0.15))
                        {
                            x = (int)Math.Ceiling(width * 0.10);
                        }
                    }
                    else // white covers 
                    {
                        
                        if (x > (width * 0.15))
                        {
                            x = (int)Math.Ceiling(width * 0.10);
                        }
                    }

                    left = x;
                    break;
                }
            }

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
                image.Write(sourceImage);
            }
        }
        catch (Exception)
        {
            // Silently fail if image processing fails
        }
    }

    public string CreateSmallImage(string coverImageUrl)
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

            // Resize image to 300px height
            using var image = new MagickImage(sourceImage);

            var size = new MagickGeometry(0, 360);

            image.Resize(size);

            // Save the result
            image.Write(destinationPath);

            // Return relative path
            return Path.Combine(destiantionFolder, filename);
        }

        return coverImageUrl; // Return original if resize fails
    }

    public string? DownloadFileFromWeb(string url)
    {
        if(url.Contains('?'))
        {
            var indexOfQuestionMark = url.IndexOf('?');
            url = url.Substring(0, indexOfQuestionMark).Trim('?');
        }

        var extention = new FileInfo(url).Extension;
        var destiantionFolder = Path.Combine(imageDir, "series", DateTime.Today.Year.ToString());
        var filename = $"{Guid.NewGuid()}{extention}";

        try
        {
            using (var client = new HttpClient())
            {
                using (var response = client.GetAsync(url))
                {
                    byte[] imageBytes =
                        response.Result.Content.ReadAsByteArrayAsync().Result;

                    var localDirectory = Path.Combine(serverRoot, destiantionFolder);
                    var localPath = Path.Combine(localDirectory, filename);

                    if (!Directory.Exists(localDirectory))
                        Directory.CreateDirectory(localDirectory);

                    File.WriteAllBytes(localPath, imageBytes);

                }
            }

            var urlPath = Path.Combine(destiantionFolder, filename);

            CropImage(urlPath);

            return urlPath;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public string SaveFlagFromCDN(string countryCode)
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
                using (var response = client.GetAsync(url))
                {
                    byte[] imageBytes =
                        response.Result.Content.ReadAsByteArrayAsync().Result;

                    var localPath = Path.Combine(localDirectory, filename);

                    if (!Directory.Exists(localDirectory))
                        Directory.CreateDirectory(localDirectory);

                    File.WriteAllBytes(localPath, imageBytes);
                }
            }
        }

        return Path.Combine(destiantionFolder, $"{countryCode}.webp");
    }
}
