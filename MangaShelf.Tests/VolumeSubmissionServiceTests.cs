using MangaShelf.BL.Dto;
using MangaShelf.BL.Services;
using MangaShelf.DAL;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MangaShelf.Tests;

public class VolumeSubmissionServiceTests : IDisposable
{
    private readonly DbContextOptions<MangaDbContext> _options = new DbContextOptionsBuilder<MangaDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

    private readonly TestDbContextFactory _factory;
    private readonly VolumeSubmissionService _service;

    public VolumeSubmissionServiceTests()
    {
        _factory = new TestDbContextFactory(_options);
        _service = new VolumeSubmissionService(_factory);
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
            await context.SaveChangesAsync();
        }

        await _service.SubmitAsync(NewSeriesSubmission(countryId), "submitter-id");

        await using var resultContext = CreateContext();
        var submission = await resultContext.VolumeSubmissions.SingleAsync();
        Assert.Equal(VolumeSubmissionStatus.Pending, submission.Status);
        Assert.Equal("submitter-id", submission.SubmittedByIdentityUserId);
        Assert.Equal("New series", submission.NewSeriesTitle);
        Assert.Equal("New publisher", submission.NewPublisherName);
        Assert.Empty(await resultContext.Publishers.ToListAsync());
        Assert.Empty(await resultContext.Series.ToListAsync());
        Assert.Empty(await resultContext.Volumes.ToListAsync());
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
            await context.SaveChangesAsync();
        }

        await _service.SubmitAsync(NewSeriesSubmission(countryId), "submitter-id");
        Guid submissionId;
        await using (var context = CreateContext())
        {
            submissionId = await context.VolumeSubmissions.Select(x => x.Id).SingleAsync();
        }

        await _service.ApproveAsync(submissionId, "admin-id");

        await using var resultContext = CreateContext();
        var submission = await resultContext.VolumeSubmissions.SingleAsync();
        var volume = await resultContext.Volumes.Include(x => x.Series).ThenInclude(x => x!.Publisher).SingleAsync();
        Assert.Equal(VolumeSubmissionStatus.Approved, submission.Status);
        Assert.Equal("admin-id", submission.ReviewedByIdentityUserId);
        Assert.Equal(volume.Id, submission.ApprovedVolumeId);
        Assert.True(volume.IsPublishedOnSite);
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
            await context.SaveChangesAsync();
        }

        await _service.SubmitAsync(new VolumeSubmissionRequestDto
        {
            SeriesId = seriesId,
            Number = 1,
            ReleaseDate = DateTimeOffset.UtcNow,
            Type = VolumeType.Physical
        }, "submitter-id");

        Guid submissionId;
        await using (var context = CreateContext())
        {
            submissionId = await context.VolumeSubmissions.Select(x => x.Id).SingleAsync();
        }

        await _service.ApproveAsync(submissionId, "admin-id");

        await using var resultContext = CreateContext();
        var volume = await resultContext.Volumes.SingleAsync();
        Assert.Equal(seriesId, volume.SeriesId);
        Assert.Single(await resultContext.Series.ToListAsync());
        Assert.Single(await resultContext.Publishers.ToListAsync());
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
            await context.SaveChangesAsync();
        }

        await _service.SubmitAsync(NewSeriesSubmission(countryId), "submitter-id");
        Guid submissionId;
        await using (var context = CreateContext())
        {
            submissionId = await context.VolumeSubmissions.Select(x => x.Id).SingleAsync();
        }

        await _service.RejectAsync(submissionId, "admin-id", "Duplicate listing");

        await using var resultContext = CreateContext();
        var submission = await resultContext.VolumeSubmissions.SingleAsync();
        Assert.Equal(VolumeSubmissionStatus.Rejected, submission.Status);
        Assert.Equal("admin-id", submission.ReviewedByIdentityUserId);
        Assert.Equal("Duplicate listing", submission.ReviewComment);
        Assert.Empty(await resultContext.Volumes.ToListAsync());
    }

    public void Dispose()
    {
        using var context = CreateContext();
        context.Database.EnsureDeleted();
    }

    private TestMangaDbContext CreateContext() => new(_options);

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
