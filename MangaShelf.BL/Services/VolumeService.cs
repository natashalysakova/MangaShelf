using AngleSharp.Text;
using MangaShelf.BL.Interfaces;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks.Sources;

namespace MangaShelf.BL.Services;

public class VolumeService(ILogger<VolumeService> logger, IVolumeRepository volumeRepository, ISeriesService seriesService) : IVolumeService
{
    public async Task CreateOrUpdateFromParsedInfoAsync(ParsedInfo volumeInfo)
    {
        var volume = await volumeRepository.FindBySeriesNameAndNumber(volumeInfo.series, volumeInfo.volumeNumber);

        if (volume == null)
        {
            var series = await seriesService.FindByName(volumeInfo.series);
            if(series == null)
            {
                series = await seriesService.CreateFromParsedVolumeInfo(volumeInfo);
            }

            volume = new Volume()
            {
                Title = volumeInfo.title,
                Series = series,
                ISBN = volumeInfo.isbn,
                Number = volumeInfo.volumeNumber,
                PreorderStart = volumeInfo.preorderStartDate,
                OneShot = volumeInfo.type.Equals("oneshot"),
            };

        }


        volume.PurchaseUrl = volumeInfo.url;
        volume.Series.Ongoing = volumeInfo.seriesStatus == "ongoing";

        if (volumeInfo.totalVolumes > 0 && volumeInfo.totalVolumes > volume.Series.TotalVolumes)
        {
            volume.Series.TotalVolumes = volumeInfo.totalVolumes;
        }
        if (volumeInfo.release is not null)
        {
            volume.ReleaseDate = volumeInfo.release;
        }

        await volumeRepository.Add(volume);

        if (volume.CoverImageUrl is null)
        {
            volume.CoverImageUrl = DownloadFileFromWeb(volumeInfo.cover, volume.Series.Id, volume.Id);
        }

        await volumeRepository.Update(volume);
    }


    private const string serverRoot = "wwwroot";
    const string imageDir = "images";
    private static string? DownloadFileFromWeb(string url, Guid seriesId, Guid volumeId)
    {
        var extention = new FileInfo(url).Extension;
        //var escapedSeriesName = seriesName.Unidecode().Replace(Path.GetInvalidFileNameChars(), string.Empty);
        var destiantionFolder = Path.Combine(imageDir, "series", seriesId.ToString());
        var filename = $"{seriesId}_{volumeId}{extention}";

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

                    System.IO.File.WriteAllBytes(localPath, imageBytes);

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
}
