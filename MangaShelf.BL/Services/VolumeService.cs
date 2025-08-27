using MangaShelf.BL.Interfaces;
using MangaShelf.BL.Mappers;
using MangaShelf.Common;
using MangaShelf.Common.Dto;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using MangaShelf.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Services;

public class VolumeService(ILogger<VolumeService> logger, IVolumeRepository volumeRepository, ISeriesService seriesService) : IVolumeService
{
    public async Task CreateOrUpdateFromParsedInfoAsync(ParsedInfo volumeInfo)
    {
        var volume = await volumeRepository.FindBySeriesNameAndNumber(volumeInfo.series, volumeInfo.volumeNumber);

        if (volume == null)
        {
            var series = await seriesService.FindByName(volumeInfo.series);
            if (series == null)
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

        if (volume.PurchaseUrl is null || volume.PurchaseUrl != volumeInfo.url)
        {
            volume.PurchaseUrl = volumeInfo.url;
        }

        volume.Series.Ongoing = volumeInfo.seriesStatus == "ongoing";
        volume.IsPreorder = volumeInfo.isPreorder;

        if (volumeInfo.totalVolumes > 0 && volumeInfo.totalVolumes > volume.Series.TotalVolumes)
        {
            volume.Series.TotalVolumes = volumeInfo.totalVolumes;
        }
        if (volumeInfo.release is not null)
        {
            volume.ReleaseDate = volumeInfo.release;
        }
        else if (!volume.IsPreorder)
        {
            volume.ReleaseDate = DateTimeOffset.Now;
        }

        if (volume.IsPreorder && volume.PreorderStart == null)
        {
            volume.PreorderStart = DateTimeOffset.Now;
        }

        if (volume.Id == Guid.Empty)
        {
            await volumeRepository.Add(volume);
            logger.LogInformation($"{volume.Series.Title} {volume.Number} added");
        }

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
        var filename = $"{volumeId}{extention}";

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

    public async Task<IEnumerable<VolumeDto>> GetAllVolumesAsync(PaginationOptions? paginationOptions = null)
    {
        var volumes = await volumeRepository.GetAllWithSeries(paginationOptions);
        return volumes.Select(v => v.ToDto());
    }

    public async Task<VolumeDto?> GetbyIdAsync(Guid id)
    {
        var volume = await volumeRepository.Get(id);
        return volume?.ToDto();
    }

    public async Task<IEnumerable<string>> FilterExistingVolumes(IEnumerable<string> volumesToParse)
    {
        if (volumesToParse == null || !volumesToParse.Any())
        {
            return Enumerable.Empty<string>();
        }

        var existingUrls = await volumeRepository
            .GetAll()
            .Where(v => v.PurchaseUrl != null && volumesToParse.Contains(v.PurchaseUrl))
            .Where(v => !v.IsPreorder)
            .Select(v => v.PurchaseUrl!)
            .ToListAsync();

        // Return URLs that are in volumesToParse but not in existingUrls
        return volumesToParse.Except(existingUrls);
    }
}
