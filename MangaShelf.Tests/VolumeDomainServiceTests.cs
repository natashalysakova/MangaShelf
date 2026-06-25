using MangaShelf.Common.Models;
using MangaShelf.DAL;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MangaShelf.Tests;

public class VolumeDomainServiceTests : IDisposable
{
    private readonly DbContextOptions<MangaDbContext> _options;

    public VolumeDomainServiceTests()
    {
        var databaseName = Guid.NewGuid().ToString();

        _options = new DbContextOptionsBuilder<MangaDbContext>()
            .UseInMemoryDatabase(databaseName)
            .EnableSensitiveDataLogging()
            .Options;
    }

    [Fact]
    public async Task FindVolumeFromParsedInfo_WhenUrlAndIsbnMatchDifferentVolumes_ReturnsUrlMatch()
    {
        await using var context = CreateContext();

        var urlMatch = CreateVolume(CreateSeries("Series A"), "Volume A", 1, purchaseUrl: "https://example.com/volume-a", isbn: "isbn-a");
        var isbnMatch = CreateVolume(CreateSeries("Series B"), "Volume B", 2, purchaseUrl: "https://example.com/volume-b", isbn: "isbn-b");

        context.Volumes.AddRange(urlMatch, isbnMatch);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = service.FindVolumeFromParsedInfo(new VolumeInfoRequest
        {
            Series = "Series C",
            VolumeNumber = 3,
            Title = "Volume C",
            Url = "https://example.com/volume-a",
            ISBN = "isbn-b"
        });

        Assert.NotNull(result);
        Assert.Equal(urlMatch.Id, result.Id);
    }

    [Fact]
    public async Task FindVolumeFromParsedInfo_WhenUrlDoesNotMatch_ReturnsIsbnMatch()
    {
        await using var context = CreateContext();

        var isbnMatch = CreateVolume(CreateSeries("Series A"), "Volume A", 1, purchaseUrl: "https://example.com/volume-a", isbn: "isbn-a");
        context.Volumes.Add(isbnMatch);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = service.FindVolumeFromParsedInfo(new VolumeInfoRequest
        {
            Series = "Other Series",
            VolumeNumber = 99,
            Title = "Other Volume",
            Url = "https://example.com/missing",
            ISBN = "isbn-a"
        });

        Assert.NotNull(result);
        Assert.Equal(isbnMatch.Id, result.Id);
    }

    [Fact]
    public async Task FindVolumeFromParsedInfo_WhenUrlAndIsbnDoNotMatch_ReturnsSeriesNumberAndTitleMatch()
    {
        await using var context = CreateContext();

        var titleMatch = CreateVolume(CreateSeries("Matched Series"), "Matched Title", 7, purchaseUrl: "https://example.com/volume-a", isbn: "isbn-a");
        context.Volumes.Add(titleMatch);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = service.FindVolumeFromParsedInfo(new VolumeInfoRequest
        {
            Series = "Matched Series",
            VolumeNumber = 7,
            Title = "Matched Title",
            Url = "https://example.com/missing",
            ISBN = "missing-isbn"
        });

        Assert.NotNull(result);
        Assert.Equal(titleMatch.Id, result.Id);
    }

    [Fact]
    public async Task FindVolumeFromParsedInfo_WhenOnlyDeletedVolumeMatches_ReturnsDeletedVolume()
    {
        await using var context = CreateContext();

        var deletedVolume = CreateVolume(CreateSeries("Deleted Series"), "Deleted Title", 5, purchaseUrl: "https://example.com/deleted", isbn: "deleted-isbn", isDeleted: true);
        context.Volumes.Add(deletedVolume);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = service.FindVolumeFromParsedInfo(new VolumeInfoRequest
        {
            Series = "Deleted Series",
            VolumeNumber = 5,
            Title = "Deleted Title",
            Url = "https://example.com/deleted",
            ISBN = "deleted-isbn"
        });

        Assert.NotNull(result);
        Assert.Equal(deletedVolume.Id, result.Id);
        Assert.True(result.IsDeleted);
    }

    [Fact]
    public async Task FindVolumeFromParsedInfo_WhenMoreThanOneVolumeHasSameIsbn_ReturnsSeriesNumberAndTitleMatch()
    {
        await using var context = CreateContext();

        var firstVolume = CreateVolume(CreateSeries("Series A"), "Volume A", 1, purchaseUrl: "https://example.com/volume-a", isbn: "shared-isbn");
        var expectedVolume = CreateVolume(CreateSeries("Series B"), "Volume B", 2, purchaseUrl: "https://example.com/volume-b", isbn: "shared-isbn");

        context.Volumes.AddRange(firstVolume, expectedVolume);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = service.FindVolumeFromParsedInfo(new VolumeInfoRequest
        {
            Series = "Series B",
            VolumeNumber = 2,
            Title = "Volume B",
            Url = "https://example.com/missing",
            ISBN = "shared-isbn"
        });

        Assert.NotNull(result);
        Assert.Equal(expectedVolume.Id, result.Id);
    }

    [Fact]
    public async Task FindVolumeFromParsedInfo_WhenMoreThanOneVolumeHasSameIsbnAndNoOtherUniqueMatch_ReturnsNull()
    {
        await using var context = CreateContext();

        var firstVolume = CreateVolume(CreateSeries("Series A"), "Volume A", 1, purchaseUrl: "https://example.com/volume-a", isbn: "shared-isbn");
        var secondVolume = CreateVolume(CreateSeries("Series B"), "Volume B", 2, purchaseUrl: "https://example.com/volume-b", isbn: "shared-isbn");

        context.Volumes.AddRange(firstVolume, secondVolume);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = service.FindVolumeFromParsedInfo(new VolumeInfoRequest
        {
            Series = "Missing Series",
            VolumeNumber = 999,
            Title = "Missing Title",
            Url = "https://example.com/missing",
            ISBN = "shared-isbn"
        });

        Assert.Null(result);
    }

    public void Dispose()
    {
        using var context = CreateContext();
        context.Database.EnsureDeleted();
    }

    private MangaDbContext CreateContext()
    {
        return new TestMangaDbContext(_options);
    }

    private static IVolumeDomainService CreateService(MangaDbContext context)
    {
        return new DomainServiceFactory(context).GetDomainService<IVolumeDomainService>();
    }

    private static Series CreateSeries(string title)
    {
        return new Series
        {
            Id = Guid.NewGuid(),
            Title = title,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "TestSystem"
        };
    }

    private static Volume CreateVolume(Series series, string title, int? number, string? purchaseUrl, string? isbn, bool isDeleted = false)
    {
        return new Volume
        {
            Id = Guid.NewGuid(),
            Series = series,
            SeriesId = series.Id,
            Title = title,
            Number = number,
            PurchaseUrl = purchaseUrl,
            ISBN = isbn,
            ReleaseDate = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "TestSystem",
            IsDeleted = isDeleted
        };
    }
}
