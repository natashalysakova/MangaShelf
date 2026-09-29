using MangaShelf.BL.Dto;
using MangaShelf.BL.Services;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace MangaShelf.Tests;

public class VolumeSubmissionServiceTests : IDisposable
{
    private readonly DbContextOptions<MangaDbContext> _options = new DbContextOptionsBuilder<MangaDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

    private readonly TestDbContextFactory _factory;
    private readonly VolumeSubmissionService _service;
    private readonly Mock<IImageFlow> _imageFlow = new();

    public VolumeSubmissionServiceTests()
    {
        _factory = new TestDbContextFactory(_options);
        _imageFlow
            .Setup(x => x.UploadAndProcessImage(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new ImageResult
            {
                OriginalImage = "images/series/submission/cover.jpg",
                CroppedImage = "images/series/submission/cover_crop.jpg",
                SmallImage = "images/small/cover_crop.jpg"
            });
        _service = new VolumeSubmissionService(_factory, _imageFlow.Object);
    }

    [Fact]
    public async Task SubmitAsync_NewPublisherAndSeries_StoresPendingSubmissionWithoutPublishingCatalogData()
    {
        var countryId = Guid.NewGuid();
        await using (var context = CreateContext())
        {
            context.Countries.Add(new Country
            {
                Id = countryId,
                Name = "Ukraine",
                CountryCode = "UA",
                FlagUrl = "flag",
                CreatedBy = "test"
            });
            await context.SaveChangesAsync(Token);
        }

        await SubmitAsync(NewSeriesSubmission(countryId), "submitter-id");

        await using var resultContext = CreateContext();
        var submission = await resultContext.VolumeSubmissions.SingleAsync(Token);
        Assert.Equal(VolumeSubmissionStatus.Pending, submission.Status);
        Assert.Equal("submitter-id", submission.SubmittedByIdentityUserId);
        Assert.Equal("New series", submission.NewSeriesTitle);
        Assert.Equal("New publisher", submission.NewPublisherName);
        Assert.Equal("images/series/submission/cover.jpg", submission.OriginalCoverUrl);
        Assert.Equal("images/series/submission/cover_crop.jpg", submission.CoverImageUrl);
        Assert.Equal("images/small/cover_crop.jpg", submission.CoverImageUrlSmall);
        Assert.Empty(await resultContext.Publishers.ToListAsync(Token));
        Assert.Empty(await resultContext.Series.ToListAsync(Token));
        Assert.Empty(await resultContext.Volumes.ToListAsync(Token));
    }

    [Fact]
    public async Task ApproveAsync_NewPublisherAndSeries_CreatesPublishedVolumeAndMarksSubmissionApproved()
    {
        var countryId = Guid.NewGuid();
        await using (var context = CreateContext())
        {
            context.Countries.Add(new Country
            {
                Id = countryId,
                Name = "Ukraine",
                CountryCode = "UA",
                FlagUrl = "flag",
                CreatedBy = "test"
            });
            await context.SaveChangesAsync(Token);
        }

        await SubmitAsync(NewSeriesSubmission(countryId), "submitter-id");
        Guid submissionId;
        await using (var context = CreateContext())
        {
            submissionId = await context.VolumeSubmissions.Select(x => x.Id).SingleAsync(Token);
        }

        await _service.ApproveAsync(submissionId, "admin-id", Token);

        await using var resultContext = CreateContext();
        var submission = await resultContext.VolumeSubmissions.SingleAsync(Token);
        var volume = await resultContext.Volumes.Include(x => x.Series).ThenInclude(x => x!.Publisher).SingleAsync(Token);
        Assert.Equal(VolumeSubmissionStatus.Approved, submission.Status);
        Assert.Equal("admin-id", submission.ReviewedByIdentityUserId);
        Assert.Equal(volume.Id, submission.ApprovedVolumeId);
        Assert.True(volume.IsPublishedOnSite);
        Assert.Equal("images/series/submission/cover.jpg", volume.OriginalCoverUrl);
        Assert.Equal("images/series/submission/cover_crop.jpg", volume.CoverImageUrl);
        Assert.Equal("images/small/cover_crop.jpg", volume.CoverImageUrlSmall);
        Assert.Equal("New series", volume.Series!.Title);
        Assert.Equal("New publisher", volume.Series.Publisher!.Name);
    }

    [Fact]
    public async Task ApproveAsync_ExistingSeries_UsesSelectedSeriesAndDoesNotCreatePublisherOrSeries()
    {
        var seriesId = Guid.NewGuid();
        var publisherId = Guid.NewGuid();
        await using (var context = CreateContext())
        {
            var publisher = new Publisher
            {
                Id = publisherId,
                Name = "Existing publisher",
                CountryId = Guid.NewGuid(),
                CreatedBy = "test"
            };
            context.Publishers.Add(publisher);
            context.Series.Add(new Series
            {
                Id = seriesId,
                Title = "Existing series",
                PublisherId = publisherId,
                Publisher = publisher,
                CreatedBy = "test"
            });
            await context.SaveChangesAsync(Token);
        }

        await SubmitAsync(new VolumeSubmissionRequestDto
        {
            SeriesId = seriesId,
            Number = 1,
            ReleaseDate = DateTimeOffset.UtcNow,
            Type = VolumeType.Physical
        }, "submitter-id");

        Guid submissionId;
        await using (var context = CreateContext())
        {
            submissionId = await context.VolumeSubmissions.Select(x => x.Id).SingleAsync(Token);
        }

        await _service.ApproveAsync(submissionId, "admin-id", Token);

        await using var resultContext = CreateContext();
        var volume = await resultContext.Volumes.SingleAsync(Token);
        Assert.Equal(seriesId, volume.SeriesId);
        Assert.Single(await resultContext.Series.ToListAsync(Token));
        Assert.Single(await resultContext.Publishers.ToListAsync(Token));
    }

    [Fact]
    public async Task ApproveAsync_NewSeriesWithExistingPublisher_CreatesSeriesAndUsesExistingPublisher()
    {
        var countryId = Guid.NewGuid();
        var publisherId = Guid.NewGuid();
        await using (var context = CreateContext())
        {
            context.Countries.Add(new Country
            {
                Id = countryId,
                Name = "Ukraine",
                CountryCode = "UA",
                FlagUrl = "flag",
                CreatedBy = "test"
            });
            context.Publishers.Add(new Publisher
            {
                Id = publisherId,
                Name = "Existing publisher",
                CountryId = countryId,
                CreatedBy = "test"
            });
            await context.SaveChangesAsync(Token);
        }

        await SubmitAsync(new VolumeSubmissionRequestDto
        {
            PublisherId = publisherId,
            NewSeriesTitle = "New series",
            ReleaseDate = DateTimeOffset.UtcNow,
            Type = VolumeType.Physical
        }, "submitter-id");

        Guid submissionId;
        await using (var context = CreateContext())
        {
            submissionId = await context.VolumeSubmissions.Select(x => x.Id).SingleAsync(Token);
        }

        await _service.ApproveAsync(submissionId, "admin-id", Token);

        await using var resultContext = CreateContext();
        var series = await resultContext.Series.Include(x => x.Publisher).SingleAsync(Token);
        Assert.Equal("New series", series.Title);
        Assert.Equal(publisherId, series.PublisherId);
        Assert.Equal("Existing publisher", series.Publisher!.Name);
        Assert.Single(await resultContext.Publishers.ToListAsync(Token));
        Assert.Single(await resultContext.Volumes.ToListAsync(Token));
    }

    [Fact]
    public async Task RejectAsync_RecordsReviewerAndCommentWithoutCreatingVolume()
    {
        var countryId = Guid.NewGuid();
        await using (var context = CreateContext())
        {
            context.Countries.Add(new Country
            {
                Id = countryId,
                Name = "Ukraine",
                CountryCode = "UA",
                FlagUrl = "flag",
                CreatedBy = "test"
            });
            await context.SaveChangesAsync(Token);
        }

        await SubmitAsync(NewSeriesSubmission(countryId), "submitter-id");
        Guid submissionId;
        await using (var context = CreateContext())
        {
            submissionId = await context.VolumeSubmissions.Select(x => x.Id).SingleAsync(Token);
        }

        await _service.RejectAsync(submissionId, "admin-id", "Duplicate listing", Token);

        await using var resultContext = CreateContext();
        var submission = await resultContext.VolumeSubmissions.SingleAsync(Token);
        Assert.Equal(VolumeSubmissionStatus.Rejected, submission.Status);
        Assert.Equal("admin-id", submission.ReviewedByIdentityUserId);
        Assert.Equal("Duplicate listing", submission.ReviewComment);
        Assert.Empty(await resultContext.Volumes.ToListAsync(Token));
    }

    [Fact]
    public async Task SubmitAsync_NonHttpPurchaseUrl_RejectsSubmission()
    {
        var seriesId = Guid.NewGuid();
        var publisherId = Guid.NewGuid();
        var countryId = Guid.NewGuid();
        await using (var context = CreateContext())
        {
            var country = new Country
            {
                Id = countryId,
                Name = "Ukraine",
                CountryCode = "UA",
                FlagUrl = "flag",
                CreatedBy = "test"
            };
            var publisher = new Publisher
            {
                Id = publisherId,
                Name = "Existing publisher",
                CountryId = countryId,
                Country = country,
                CreatedBy = "test"
            };
            context.Countries.Add(country);
            context.Publishers.Add(publisher);
            context.Series.Add(new Series
            {
                Id = seriesId,
                Title = "Existing series",
                PublisherId = publisherId,
                Publisher = publisher,
                CreatedBy = "test"
            });
            await context.SaveChangesAsync(Token);
        }

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SubmitAsync(new VolumeSubmissionRequestDto
            {
                SeriesId = seriesId,
                Number = 1,
                PurchaseUrl = "javascript:alert(1)",
                ReleaseDate = DateTimeOffset.UtcNow
            }, "submitter-id", new MemoryStream([1]), "cover.jpg", Token));

        Assert.Equal("URLs must use http or https.", exception.Message);
        await using var resultContext = CreateContext();
        Assert.Empty(await resultContext.VolumeSubmissions.ToListAsync(Token));
    }

    [Fact]
    public async Task SubmitAsync_WithoutCover_RejectsSubmission()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SubmitAsync(new VolumeSubmissionRequestDto(), "submitter-id", null, null, Token));

        Assert.Equal("A cover image is required.", exception.Message);
        _imageFlow.Verify(
            x => x.UploadAndProcessImage(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    public void Dispose()
    {
        using var context = CreateContext();
        context.Database.EnsureDeleted();
    }

    private TestMangaDbContext CreateContext() => new(_options);
    private static CancellationToken Token => TestContext.Current.CancellationToken;
    private Task SubmitAsync(VolumeSubmissionRequestDto request, string submittedBy) =>
        _service.SubmitAsync(request, submittedBy, new MemoryStream([1]), "cover.jpg", Token);

    private static VolumeSubmissionRequestDto NewSeriesSubmission(Guid countryId) => new()
    {
        NewSeriesTitle = "New series",
        NewPublisherName = "New publisher",
        NewPublisherCountryId = countryId,
        Title = "Volume 1",
        Number = 1,
        ISBN = "978-1-234",
        ReleaseDate = DateTimeOffset.UtcNow,
        Type = VolumeType.Physical
    };
}
