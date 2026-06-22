using MangaShelf.DAL;
using MangaShelf.DAL.Interceptors;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MangaShelf.Tests;

public class WriteVolumeHistoryInterceptorTests : IDisposable
{
    private readonly string _databaseName;

    public WriteVolumeHistoryInterceptorTests()
    {
        _databaseName = Guid.NewGuid().ToString();
    }

    [Fact]
    public async Task SaveChanges_AddsReleasedHistory_ForNewNonPreorderVolume()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var releaseDate = new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero);

        using var context = CreateContext();

        context.Volumes.Add(new Volume
        {
            Title = "Volume 1",
            Number = 1,
            SeriesId = Guid.NewGuid(),
            IsPreorder = false,
            ReleaseDate = releaseDate,
            CreatedBy = "tests"
        });

        await context.SaveChangesAsync(cancellationToken);

        var history = await context.VolumeHistory.SingleAsync(cancellationToken);

        Assert.Equal(HistoryEventType.Released, history.EventType);
        Assert.Equal(string.Empty, history.OldValue);
        Assert.Equal(bool.FalseString, history.NewValue);
    }

    [Fact]
    public async Task SaveChanges_AddsSinglePreorderStartedHistory_ForNewPreorderVolume()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var context = CreateContext();

        context.Volumes.Add(new Volume
        {
            Title = "Volume 2",
            Number = 2,
            SeriesId = Guid.NewGuid(),
            IsPreorder = true,
            CreatedBy = "tests",
            ReleaseDate = new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero)
        });

        await context.SaveChangesAsync(cancellationToken);

        var histories = await context.VolumeHistory.ToListAsync(cancellationToken);

        var history = Assert.Single(histories);
        Assert.Equal(HistoryEventType.PreorderStarted, history.EventType);
    }

    [Fact]
    public async Task SaveChanges_ForNewPreorderVolumeWithReleaseDate_DoesNotAddReleaseDateChangedHistory()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var releaseDate = new DateTimeOffset(2026, 3, 10, 0, 0, 0, TimeSpan.Zero);

        using var context = CreateContext();

        context.Volumes.Add(new Volume
        {
            Title = "Volume 2.1",
            Number = 21,
            SeriesId = Guid.NewGuid(),
            IsPreorder = true,
            ReleaseDate = releaseDate,
            CreatedBy = "tests"
        });

        await context.SaveChangesAsync(cancellationToken);

        var histories = await context.VolumeHistory.ToListAsync(cancellationToken);

        var history = Assert.Single(histories);
        Assert.Equal(HistoryEventType.PreorderStarted, history.EventType);
        Assert.DoesNotContain(histories, x => x.EventType == HistoryEventType.ReleaseDateChanged);
    }

    [Fact]
    public async Task SaveChanges_AddsSingleReleaseDateChangedHistory_WhenPreorderReleaseDateChanges()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var initialReleaseDate = new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero);
        var changedReleaseDate = new DateTimeOffset(2026, 2, 10, 0, 0, 0, TimeSpan.Zero);

        using var context = CreateContext();

        var volume = new Volume
        {
            Title = "Volume 3",
            Number = 3,
            SeriesId = Guid.NewGuid(),
            IsPreorder = true,
            ReleaseDate = initialReleaseDate,
            CreatedBy = "tests"
        };

        context.Volumes.Add(volume);
        await context.SaveChangesAsync(cancellationToken);

        context.VolumeHistory.RemoveRange(context.VolumeHistory);
        await context.SaveChangesAsync(cancellationToken);

        volume.ReleaseDate = changedReleaseDate;
        await context.SaveChangesAsync(cancellationToken);

        var histories = await context.VolumeHistory.ToListAsync(cancellationToken);

        var history = Assert.Single(histories);
        Assert.Equal(HistoryEventType.ReleaseDateChanged, history.EventType);
    }

    [Fact]
    public async Task SaveChanges_AddsSinglePreorderStartedHistory_WhenVolumeBecomesPreorderAgain()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var context = CreateContext();

        var volume = new Volume
        {
            Title = "Volume 4",
            Number = 4,
            SeriesId = Guid.NewGuid(),
            IsPreorder = false,
            CreatedBy = "tests",
            ReleaseDate = new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero)
        };

        context.Volumes.Add(volume);
        await context.SaveChangesAsync(cancellationToken);

        context.VolumeHistory.RemoveRange(context.VolumeHistory);
        await context.SaveChangesAsync(cancellationToken);

        volume.IsPreorder = true;
        await context.SaveChangesAsync(cancellationToken);

        var histories = await context.VolumeHistory.ToListAsync(cancellationToken);

        var history = Assert.Single(histories);
        Assert.Equal(HistoryEventType.PreorderStarted, history.EventType);
    }

    [Fact]
    public async Task SaveChanges_WhenNonPreorderReleaseDateChanges_DoesNotCreateHistory()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var initialReleaseDate = new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero);
        var changedReleaseDate = new DateTimeOffset(2026, 2, 10, 0, 0, 0, TimeSpan.Zero);

        using var context = CreateContext();

        var volume = new Volume
        {
            Title = "Volume 5",
            Number = 5,
            SeriesId = Guid.NewGuid(),
            IsPreorder = false,
            ReleaseDate = initialReleaseDate,
            CreatedBy = "tests"
        };

        context.Volumes.Add(volume);
        await context.SaveChangesAsync(cancellationToken);

        context.VolumeHistory.RemoveRange(context.VolumeHistory);
        await context.SaveChangesAsync(cancellationToken);

        volume.ReleaseDate = changedReleaseDate;
        await context.SaveChangesAsync(cancellationToken);

        var histories = await context.VolumeHistory.ToListAsync(cancellationToken);

        Assert.Empty(histories);
    }

    [Fact]
    public async Task SaveChanges_WhenPreorderBecomesReleased_DoesNotCreateReleaseDateChanged()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var initialReleaseDate = new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero);
        var changedReleaseDate = new DateTimeOffset(2026, 2, 10, 0, 0, 0, TimeSpan.Zero);

        using var context = CreateContext();

        var volume = new Volume
        {
            Title = "Volume 6",
            Number = 6,
            SeriesId = Guid.NewGuid(),
            IsPreorder = true,
            ReleaseDate = initialReleaseDate,
            CreatedBy = "tests"
        };

        context.Volumes.Add(volume);
        await context.SaveChangesAsync(cancellationToken);

        context.VolumeHistory.RemoveRange(context.VolumeHistory);
        await context.SaveChangesAsync(cancellationToken);

        volume.IsPreorder = false;
        volume.ReleaseDate = changedReleaseDate;
        await context.SaveChangesAsync(cancellationToken);

        var histories = await context.VolumeHistory.ToListAsync(cancellationToken);

        var history = Assert.Single(histories);
        Assert.Equal(HistoryEventType.Released, history.EventType);
        Assert.DoesNotContain(histories, x => x.EventType == HistoryEventType.ReleaseDateChanged);
    }

    public void Dispose()
    {
        using var context = CreateContext();
        context.Database.EnsureDeleted();
    }

    private MangaDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MangaDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .AddInterceptors(new WriteVolumeHistoryInterceptor())
            .AddInterceptors(new AuditInterceptor())
            .Options;

        return new MangaDbContext(options);
    }
}
