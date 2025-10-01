using ImageMagick;

namespace MangaShelf.Common.Interfaces;

public interface IImageManager
{
    string CreateSmallImage(string coverImageUrl);
    string? DownloadFileFromWeb(string url);
    string SaveFlagFromCDN(string countryCode);
}

public class ImageManager : IImageManager
{
    private const string serverRoot = "wwwroot";
    const string imageDir = "images";

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
