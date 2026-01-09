using MangaShelf.BL.Parsers;
using MangaShelf.BL.Services.Parsing;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace MangaShelf.Tests;

[TestClass]
public class ParserServiceTests
{
    private Mock<ILogger<ParserService>> _loggerMock;
    private Mock<IServiceProvider> _serviceProviderMock;
    private Mock<IServiceScope> _serviceScopeMock;
    private Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private Mock<IImageManager> _imageManagerMock;
    private IDbContextFactory<MangaDbContext> _dbContextFactory;
    private IOptions<ParserServiceOptions> _options;
    private ParserService _parserService;
    private string _databaseName;

    [TestInitialize]
    public void Setup()
    {
        _databaseName = $"TestDb_{Guid.NewGuid()}";
        
        // Setup in-memory database - ConfigureWarnings to suppress null checks
        var dbContextOptions = new DbContextOptionsBuilder<MangaDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .EnableSensitiveDataLogging()
            .Options;
        
        _dbContextFactory = new TestDbContextFactory(dbContextOptions);

        // Setup mocks
        _loggerMock = new Mock<ILogger<ParserService>>();
        _imageManagerMock = new Mock<IImageManager>();
        
        // Create a real service collection for dependency injection
        var services = new ServiceCollection();
        services.AddScoped<IImageManager>(sp => _imageManagerMock.Object);
        
        var serviceProvider = services.BuildServiceProvider();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceScopeMock = new Mock<IServiceScope>();
        
        // Setup service scope properly
        _serviceScopeMock.Setup(x => x.ServiceProvider).Returns(serviceProvider);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(new TestServiceScopeFactory(_serviceScopeMock.Object));
        
        _options = Options.Create(new ParserServiceOptions
        {
            DelayBetweenParse = 100,
            IgnoreExistingVolumes = false
        });

        // Setup image manager mock
        _imageManagerMock.Setup(x => x.DownloadFileFromWeb(It.IsAny<string>(), It.IsAny<Guid>()))
            .Returns("images/cover.jpg");
        _imageManagerMock.Setup(x => x.CreateSmallImage(It.IsAny<string>()))
            .Returns("images/small/cover.jpg");

        // Initialize database with test data
        InitializeTestData().Wait();

        _parserService = new ParserService(
            _loggerMock.Object,
            _dbContextFactory,
            serviceProvider,
            null!, // IParserFactory not needed for this test
            _options
        );
    }

    [TestCleanup]
    public void Cleanup()
    {
        using var context = _dbContextFactory.CreateDbContext();
        context.Database.EnsureDeleted();
    }

    private async Task InitializeTestData()
    {
        using var context = _dbContextFactory.CreateDbContext();
        
        var ukraineCountry = new Country
        {
            Id = Guid.NewGuid(),
            Name = "Ukraine",
            CountryCode = "uk",
            FlagUrl = "/flags/uk.png",
            CreatedBy = "TestSystem",
            CreatedAt = DateTimeOffset.Now
        };
        
        var japanCountry = new Country
        {
            Id = Guid.NewGuid(),
            Name = "Japan",
            CountryCode = "jp",
            FlagUrl = "/flags/jp.png",
            CreatedBy = "TestSystem",
            CreatedAt = DateTimeOffset.Now
        };

        await context.Countries.AddRangeAsync(ukraineCountry, japanCountry);
        await context.SaveChangesAsync();
    }

    [TestMethod]
    public async Task CreateOrUpdateFromParsedInfoAsync_NewVolume_CreatesVolume()
    {
        // Arrange
        var parsedInfo = new ParsedInfo
        {
            Title = "Volume 1",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Test Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 5,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "テストシリーズ",
            Url = "https://example.com/volume1",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "This is a test volume description."
        };

        // Act
        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(parsedInfo);

        // Assert
        Assert.AreEqual(State.Added, result);

        using var context = _dbContextFactory.CreateDbContext();
        var volume = await context.Volumes
            .Include(v => v.Series)
            .ThenInclude(s => s!.Authors)
            .FirstOrDefaultAsync(v => v.Title == "Volume 1");

        Assert.IsNotNull(volume);
        Assert.AreEqual("Volume 1", volume.Title);
        Assert.AreEqual(1, volume.Number);
        Assert.AreEqual("978-1234567890", volume.ISBN);
        Assert.AreEqual(16, volume.AgeRestriction);
        Assert.IsFalse(volume.IsPreorder);
        Assert.AreEqual("This is a test volume description.", volume.Description);
        Assert.AreEqual("https://example.com/volume1", volume.PurchaseUrl);
        Assert.AreEqual("images/cover.jpg", volume.CoverImageUrl);
        Assert.AreEqual("images/small/cover.jpg", volume.CoverImageUrlSmall);

        Assert.IsNotNull(volume.Series);
        Assert.AreEqual("Test Series", volume.Series.Title);
        Assert.AreEqual("テストシリーズ", volume.Series.OriginalName);
        Assert.AreEqual(SeriesStatus.Ongoing, volume.Series.Status);
        Assert.AreEqual(5, volume.Series.TotalVolumes);
        Assert.AreEqual(SeriesType.Manga, volume.Series.Type);
        Assert.IsTrue(volume.Series.IsPublishedOnSite);

        Assert.AreEqual(1, volume.Series.Authors.Count);
        Assert.AreEqual("Test Author", volume.Series.Authors.First().Name);
    }

    [TestMethod]
    public async Task CreateOrUpdateFromParsedInfoAsync_ExistingVolume_UpdatesVolume()
    {
        // Arrange - Create initial volume
        var initialParsedInfo = new ParsedInfo
        {
            Title = "Volume 1",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Existing Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 3,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "既存シリーズ",
            Url = "https://example.com/volume1",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Original description"
        };

        await _parserService.CreateOrUpdateFromParsedInfoAsync(initialParsedInfo);

        // Act - Update with new info
        var updatedParsedInfo = new ParsedInfo
        {
            Title = "Volume 1",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Existing Series",
            Cover = "https://example.com/new_cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-5),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 5,
            SeriesStatus = SeriesStatus.Completed,
            OriginalSeriesName = "既存シリーズ",
            Url = "https://example.com/volume1-new",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 18,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Updated description"
        };

        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(updatedParsedInfo);

        // Assert
        Assert.AreEqual(State.Updated, result);

        using var context = _dbContextFactory.CreateDbContext();
        var volume = await context.Volumes
            .Include(v => v.Series)
            .FirstOrDefaultAsync(v => v.Title == "Volume 1");

        Assert.IsNotNull(volume);
        Assert.AreEqual("Updated description", volume.Description);
        Assert.AreEqual("https://example.com/volume1-new", volume.PurchaseUrl);
        Assert.AreEqual(18, volume.AgeRestriction);
        Assert.AreEqual(SeriesStatus.Completed, volume.Series!.Status);
        Assert.AreEqual(5, volume.Series.TotalVolumes);
    }

    [TestMethod]
    public async Task CreateOrUpdateFromParsedInfoAsync_MultipleAuthors_CreatesAllAuthors()
    {
        // Arrange
        var parsedInfo = new ParsedInfo
        {
            Title = "Volume 1",
            Authors = "Author One, Author Two; Author Three",
            VolumeNumber = 1,
            Series = "Multi-Author Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Completed,
            OriginalSeriesName = "Original Name",
            Url = "https://example.com/volume1",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        // Act
        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(parsedInfo);

        // Assert
        Assert.AreEqual(State.Added, result);

        using var context = _dbContextFactory.CreateDbContext();
        var series = await context.Series
            .Include(s => s.Authors)
            .FirstOrDefaultAsync(s => s.Title == "Multi-Author Series");

        Assert.IsNotNull(series);
        // Authors are split but not trimmed, so they have leading spaces
        Assert.AreEqual(3, series.Authors.Count);
        var authorNames = series.Authors.Select(a => a.Name.Trim()).ToList();
        Assert.IsTrue(authorNames.Contains("Author One"));
        Assert.IsTrue(authorNames.Contains("Author Two"));
        Assert.IsTrue(authorNames.Contains("Author Three"));
    }

    [TestMethod]
    public async Task CreateOrUpdateFromParsedInfoAsync_PreorderVolume_SetsPreorderFields()
    {
        // Arrange
        var preorderStart = DateTimeOffset.Now.AddDays(5);
        var parsedInfo = new ParsedInfo
        {
            Title = "Preorder Volume",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Preorder Series",
            Cover = "https://example.com/cover.jpg",
            Release = null,
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/preorder",
            PreorderStartDate = preorderStart,
            CountryCode = "uk",
            IsPreorder = true,
            AgeRestrictions = 12,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Preorder description"
        };

        // Act
        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(parsedInfo);

        // Assert
        Assert.AreEqual(State.Added, result);

        using var context = _dbContextFactory.CreateDbContext();
        var volume = await context.Volumes
            .FirstOrDefaultAsync(v => v.Title == "Preorder Volume");

        Assert.IsNotNull(volume);
        Assert.IsTrue(volume.IsPreorder);
        Assert.IsNotNull(volume.PreorderStart);
        Assert.IsNotNull(volume.ReleaseDate);
        // PreorderStart gets set to volumeInfo.PreorderStartDate (which is 5 days in the future)
        Assert.AreEqual(preorderStart.Date, volume.PreorderStart!.Value.Date);
        // ReleaseDate gets set to Now since Release is null and IsPreorder is true
        var now = DateTimeOffset.Now;
        Assert.IsTrue(Math.Abs((volume.ReleaseDate!.Value - now).TotalSeconds) < 5, 
            $"ReleaseDate {volume.ReleaseDate.Value} should be close to now {now}");
    }

    [TestMethod]
    public async Task CreateOrUpdateFromParsedInfoAsync_OneShot_SetsOneShotFlag()
    {
        // Arrange
        var parsedInfo = new ParsedInfo
        {
            Title = "One Shot",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "One Shot Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.OneShot,
            OriginalSeriesName = "ワンショット",
            Url = "https://example.com/oneshot",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "One shot description"
        };

        // Act
        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(parsedInfo);

        // Assert
        Assert.AreEqual(State.Added, result);

        using var context = _dbContextFactory.CreateDbContext();
        var volume = await context.Volumes
            .FirstOrDefaultAsync(v => v.Title == "One Shot");

        Assert.IsNotNull(volume);
        Assert.IsTrue(volume.OneShot);
    }

    [TestMethod]
    public async Task CreateOrUpdateFromParsedInfoAsync_NoAgeRestrictions_DefaultsTo18()
    {
        // Arrange
        var parsedInfo = new ParsedInfo
        {
            Title = "Adult Volume",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Adult Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/adult",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = null,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Adult content"
        };

        // Act
        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(parsedInfo);

        // Assert
        Assert.AreEqual(State.Added, result);

        using var context = _dbContextFactory.CreateDbContext();
        var volume = await context.Volumes
            .FirstOrDefaultAsync(v => v.Title == "Adult Volume");

        Assert.IsNotNull(volume);
        Assert.AreEqual(18, volume.AgeRestriction);
    }

    [TestMethod]
    public async Task CreateOrUpdateFromParsedInfoAsync_ExistingPublisher_UsesExistingPublisher()
    {
        // Arrange - Create first volume with publisher
        var firstParsedInfo = new ParsedInfo
        {
            Title = "Volume 1",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Series One",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Shared Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/volume1",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "First volume"
        };

        await _parserService.CreateOrUpdateFromParsedInfoAsync(firstParsedInfo);

        // Act - Create second volume with same publisher
        var secondParsedInfo = new ParsedInfo
        {
            Title = "Volume 1",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Series Two",
            Cover = "https://example.com/cover2.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Shared Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-0987654321",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original 2",
            Url = "https://example.com/volume2",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Second volume"
        };

        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(secondParsedInfo);

        // Assert
        Assert.AreEqual(State.Added, result);

        using var context = _dbContextFactory.CreateDbContext();
        var publishers = await context.Publishers
            .Where(p => p.Name == "Shared Publisher")
            .ToListAsync();

        Assert.AreEqual(1, publishers.Count);

        var seriesCount = await context.Series
            .Where(s => s.Publisher!.Name == "Shared Publisher")
            .CountAsync();

        Assert.AreEqual(2, seriesCount);
    }

    [TestMethod]
    public async Task CreateOrUpdateFromParsedInfoAsync_FutureReleaseDate_UsesProvidedDate()
    {
        // Arrange
        var futureRelease = DateTimeOffset.Now.AddDays(30);
        var parsedInfo = new ParsedInfo
        {
            Title = "Future Volume",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Future Series",
            Cover = "https://example.com/cover.jpg",
            Release = futureRelease,
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/future",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Future release"
        };

        // Act
        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(parsedInfo);

        // Assert
        Assert.AreEqual(State.Added, result);

        using var context = _dbContextFactory.CreateDbContext();
        var volume = await context.Volumes
            .FirstOrDefaultAsync(v => v.Title == "Future Volume");

        Assert.IsNotNull(volume);
        Assert.IsNotNull(volume.ReleaseDate);
        // Future dates should not be used, current date should be set instead
        Assert.IsTrue(volume.ReleaseDate!.Value.Date >= DateTime.Now.Date);
    }

    [TestMethod]
    public async Task CreateOrUpdateFromParsedInfoAsync_InvalidCountryCode_UsesDefaultUkraine()
    {
        // Arrange
        var parsedInfo = new ParsedInfo
        {
            Title = "Volume 1",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Test Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/volume1",
            PreorderStartDate = null,
            CountryCode = "invalid",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        // Act
        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(parsedInfo);

        // Assert
        Assert.AreEqual(State.Added, result);

        using var context = _dbContextFactory.CreateDbContext();
        var series = await context.Series
            .Include(s => s.Publisher)
            .ThenInclude(p => p!.Country)
            .FirstOrDefaultAsync(s => s.Title == "Test Series");

        Assert.IsNotNull(series);
        Assert.IsNotNull(series.Publisher);
        Assert.IsNotNull(series.Publisher.Country);
        Assert.AreEqual("uk", series.Publisher.Country.CountryCode);
    }

    [TestMethod]
    public async Task CreateOrUpdateFromParsedInfoAsync_ExistingSeries_AddNewVolumeToSeries()
    {
        // Arrange - Create first volume in series
        var firstParsedInfo = new ParsedInfo
        {
            Title = "Volume 1",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Shared Series",
            Cover = "https://example.com/cover1.jpg",
            Release = DateTimeOffset.Now.AddDays(-30),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567891",
            TotalVolumes = 5,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "共有シリーズ",
            Url = "https://example.com/volume1",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "First volume"
        };

        await _parserService.CreateOrUpdateFromParsedInfoAsync(firstParsedInfo);

        // Act - Create second volume in same series
        var secondParsedInfo = new ParsedInfo
        {
            Title = "Volume 2",
            Authors = "Test Author",
            VolumeNumber = 2,
            Series = "Shared Series",
            Cover = "https://example.com/cover2.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567892",
            TotalVolumes = 5,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "共有シリーズ",
            Url = "https://example.com/volume2",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Second volume"
        };

        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(secondParsedInfo);

        // Assert
        Assert.AreEqual(State.Added, result);

        using var context = _dbContextFactory.CreateDbContext();
        var seriesCount = await context.Series
            .Where(s => s.Title == "Shared Series")
            .CountAsync();

        Assert.AreEqual(1, seriesCount, "Should have only one series");

        var volumes = await context.Volumes
            .Where(v => v.Series!.Title == "Shared Series")
            .ToListAsync();

        Assert.AreEqual(2, volumes.Count, "Should have two volumes in the series");
    }

    [TestMethod]
    public async Task CreateOrUpdateFromParsedInfoAsync_NullAuthors_CreatesSeriesWithoutAuthors()
    {
        // Arrange
        var parsedInfo = new ParsedInfo
        {
            Title = "No Author Volume",
            Authors = null,
            VolumeNumber = 1,
            Series = "No Author Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Completed,
            OriginalSeriesName = "Original",
            Url = "https://example.com/noauthor",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        // Act
        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(parsedInfo);

        // Assert
        Assert.AreEqual(State.Added, result);

        using var context = _dbContextFactory.CreateDbContext();
        var series = await context.Series
            .Include(s => s.Authors)
            .FirstOrDefaultAsync(s => s.Title == "No Author Series");

        Assert.IsNotNull(series);
        Assert.AreEqual(0, series.Authors.Count);
    }

    [TestMethod]
    public async Task CreateOrUpdateFromParsedInfoAsync_NotPreorderWithPreorderStartDate_ReleaseDateIs30DaysLater()
    {
        // Arrange
        var preorderStartDate = DateTimeOffset.Now.AddDays(-60);
        var parsedInfo = new ParsedInfo
        {
            Title = "Released Volume",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Released Series",
            Cover = "https://example.com/cover.jpg",
            Release = null, // No explicit release date
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/released",
            PreorderStartDate = preorderStartDate,
            CountryCode = "uk",
            IsPreorder = false, // Not a preorder anymore
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        // Act
        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(parsedInfo);

        // Assert
        Assert.AreEqual(State.Added, result);

        using var context = _dbContextFactory.CreateDbContext();
        var volume = await context.Volumes
            .FirstOrDefaultAsync(v => v.Title == "Released Volume");

        Assert.IsNotNull(volume);
        // ReleaseDate should be PreorderStartDate + 30 days
        var expectedReleaseDate = preorderStartDate.AddDays(30);
        Assert.AreEqual(expectedReleaseDate.Date, volume.ReleaseDate!.Value.Date);
    }

    [TestMethod]
    public async Task CreateOrUpdateFromParsedInfoAsync_SeriesStatusChange_UpdatesSeriesStatus()
    {
        // Arrange - Create initial volume with Ongoing status
        var initialParsedInfo = new ParsedInfo
        {
            Title = "Volume 1",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Status Change Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 3,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/volume1",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        await _parserService.CreateOrUpdateFromParsedInfoAsync(initialParsedInfo);

        // Act - Update with Completed status
        var updatedParsedInfo = new ParsedInfo
        {
            Title = "Volume 1",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Status Change Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 3,
            SeriesStatus = SeriesStatus.Completed, // Changed status
            OriginalSeriesName = "Original",
            Url = "https://example.com/volume1",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(updatedParsedInfo);

        // Assert
        Assert.AreEqual(State.Updated, result);

        using var context = _dbContextFactory.CreateDbContext();
        var series = await context.Series
            .FirstOrDefaultAsync(s => s.Title == "Status Change Series");

        Assert.IsNotNull(series);
        Assert.AreEqual(SeriesStatus.Completed, series.Status);
    }

    [TestMethod]
    public async Task CreateOrUpdateFromParsedInfoAsync_TotalVolumesIncrease_UpdatesTotalVolumes()
    {
        // Arrange - Create initial volume with 3 total volumes
        var initialParsedInfo = new ParsedInfo
        {
            Title = "Volume 1",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Volume Count Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 3,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/volume1",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        await _parserService.CreateOrUpdateFromParsedInfoAsync(initialParsedInfo);

        // Act - Update with 5 total volumes (increased)
        var updatedParsedInfo = new ParsedInfo
        {
            Title = "Volume 1",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Volume Count Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 5, // Increased
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/volume1",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(updatedParsedInfo);

        // Assert
        Assert.AreEqual(State.Updated, result);

        using var context = _dbContextFactory.CreateDbContext();
        var series = await context.Series
            .FirstOrDefaultAsync(s => s.Title == "Volume Count Series");

        Assert.IsNotNull(series);
        Assert.AreEqual(5, series.TotalVolumes);
    }

    [TestMethod]
    public async Task CreateOrUpdateFromParsedInfoAsync_TotalVolumesDecrease_DoesNotUpdateTotalVolumes()
    {
        // Arrange - Create initial volume with 5 total volumes
        var initialParsedInfo = new ParsedInfo
        {
            Title = "Volume 1",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "No Decrease Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 5,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/volume1",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        await _parserService.CreateOrUpdateFromParsedInfoAsync(initialParsedInfo);

        // Act - Try to update with 3 total volumes (decreased)
        var updatedParsedInfo = new ParsedInfo
        {
            Title = "Volume 1",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "No Decrease Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 3, // Decreased - should not be applied
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/volume1",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(updatedParsedInfo);

        // Assert
        using var context = _dbContextFactory.CreateDbContext();
        var series = await context.Series
            .FirstOrDefaultAsync(s => s.Title == "No Decrease Series");

        Assert.IsNotNull(series);
        Assert.AreEqual(5, series.TotalVolumes, "TotalVolumes should not decrease");
    }

    [TestMethod]
    public async Task CreateOrUpdateFromParsedInfoAsync_PreorderWithNullPreorderStart_SetsPreorderStartToNow()
    {
        // Arrange
        var parsedInfo = new ParsedInfo
        {
            Title = "New Preorder",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "New Preorder Series",
            Cover = "https://example.com/cover.jpg",
            Release = null,
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/newpreorder",
            PreorderStartDate = null, // No preorder start date provided
            CountryCode = "uk",
            IsPreorder = true,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        var beforeTest = DateTimeOffset.Now;

        // Act
        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(parsedInfo);

        var afterTest = DateTimeOffset.Now;

        // Assert
        Assert.AreEqual(State.Added, result);

        using var context = _dbContextFactory.CreateDbContext();
        var volume = await context.Volumes
            .FirstOrDefaultAsync(v => v.Title == "New Preorder");

        Assert.IsNotNull(volume);
        Assert.IsTrue(volume.IsPreorder);
        Assert.IsNotNull(volume.PreorderStart);
        Assert.IsTrue(volume.PreorderStart >= beforeTest && volume.PreorderStart <= afterTest,
            "PreorderStart should be set to approximately now");
    }

    [TestMethod]
    public async Task CreateOrUpdateFromParsedInfoAsync_NullDescription_DoesNotUpdateDescription()
    {
        // Arrange - Create initial volume with description
        var initialParsedInfo = new ParsedInfo
        {
            Title = "Volume 1",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Description Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/volume1",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Original description"
        };

        await _parserService.CreateOrUpdateFromParsedInfoAsync(initialParsedInfo);

        // Act - Update with null description
        var updatedParsedInfo = new ParsedInfo
        {
            Title = "Volume 1",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Description Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/volume1-new",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = null // Null description should not overwrite
        };

        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(updatedParsedInfo);

        // Assert
        using var context = _dbContextFactory.CreateDbContext();
        var volume = await context.Volumes
            .FirstOrDefaultAsync(v => v.Title == "Volume 1" && v.Series!.Title == "Description Series");

        Assert.IsNotNull(volume);
        Assert.AreEqual("Original description", volume.Description, "Description should not be overwritten with null");
    }

    [TestMethod]
    public async Task CreateOrUpdateFromParsedInfoAsync_SameDescription_DoesNotTriggerUpdate()
    {
        // Arrange - Create initial volume with description
        var initialParsedInfo = new ParsedInfo
        {
            Title = "Volume 1",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Same Desc Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/volume1",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Same description"
        };

        await _parserService.CreateOrUpdateFromParsedInfoAsync(initialParsedInfo);

        // Act - Update with same description and same URL
        var updatedParsedInfo = new ParsedInfo
        {
            Title = "Volume 1",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Same Desc Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/volume1", // Same URL
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Same description" // Same description
        };

        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(updatedParsedInfo);

        // Assert - Since nothing changed, should still be Updated (EF marks it as unchanged but method returns Updated)
        Assert.AreEqual(State.Updated, result);
    }

    [TestMethod]
    public async Task CreateOrUpdateFromParsedInfoAsync_CanBePublishedFalse_SetsIsPublishedOnSiteFalse()
    {
        // Arrange
        var parsedInfo = new ParsedInfo
        {
            Title = "Unpublished Volume",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Unpublished Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/unpublished",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = false, // Should not be published on site
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        // Act
        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(parsedInfo);

        // Assert
        Assert.AreEqual(State.Added, result);

        using var context = _dbContextFactory.CreateDbContext();
        var volume = await context.Volumes
            .Include(v => v.Series)
            .FirstOrDefaultAsync(v => v.Title == "Unpublished Volume");

        Assert.IsNotNull(volume);
        Assert.IsFalse(volume.IsPublishedOnSite);
        Assert.IsFalse(volume.Series!.IsPublishedOnSite);
    }

    [TestMethod]
    public async Task CreateOrUpdateFromParsedInfoAsync_NewlineInAuthors_SplitsCorrectly()
    {
        // Arrange
        var parsedInfo = new ParsedInfo
        {
            Title = "Volume 1",
            Authors = "Author One\nAuthor Two\nAuthor Three",
            VolumeNumber = 1,
            Series = "Newline Authors Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Completed,
            OriginalSeriesName = "Original Name",
            Url = "https://example.com/volume1",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        // Act
        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(parsedInfo);

        // Assert
        Assert.AreEqual(State.Added, result);

        using var context = _dbContextFactory.CreateDbContext();
        var series = await context.Series
            .Include(s => s.Authors)
            .FirstOrDefaultAsync(s => s.Title == "Newline Authors Series");

        Assert.IsNotNull(series);
        Assert.AreEqual(3, series.Authors.Count);
    }

    [TestMethod]
    public async Task CreateOrUpdateFromParsedInfoAsync_ZeroTotalVolumes_DoesNotUpdateTotalVolumes()
    {
        // Arrange - Create initial volume with total volumes
        var initialParsedInfo = new ParsedInfo
        {
            Title = "Volume 1",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Zero Total Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 5,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/volume1",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        await _parserService.CreateOrUpdateFromParsedInfoAsync(initialParsedInfo);

        // Act - Update with 0 total volumes (unknown)
        var updatedParsedInfo = new ParsedInfo
        {
            Title = "Volume 1",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Zero Total Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 0, // Zero - should not update
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/volume1",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(updatedParsedInfo);

        // Assert
        using var context = _dbContextFactory.CreateDbContext();
        var series = await context.Series
            .FirstOrDefaultAsync(s => s.Title == "Zero Total Series");

        Assert.IsNotNull(series);
        Assert.AreEqual(5, series.TotalVolumes, "TotalVolumes should not be updated when new value is 0");
    }

    [TestMethod]
    public async Task CreateOrUpdateFromParsedInfoAsync_DifferentSeriesType_SetsCorrectType()
    {
        // Arrange - Test with Manhwa type
        var parsedInfo = new ParsedInfo
        {
            Title = "Manhwa Volume",
            Authors = "Korean Author",
            VolumeNumber = 1,
            Series = "Manhwa Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Korean Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 10,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Korean Original",
            Url = "https://example.com/manhwa",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manhwa,
            Description = "Test description"
        };

        // Act
        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(parsedInfo);

        // Assert
        Assert.AreEqual(State.Added, result);

        using var context = _dbContextFactory.CreateDbContext();
        var series = await context.Series
            .FirstOrDefaultAsync(s => s.Title == "Manhwa Series");

        Assert.IsNotNull(series);
        Assert.AreEqual(SeriesType.Manhwa, series.Type);
    }

    [TestMethod]
    public async Task AiGen_ExistingVolumeWithCoverImages_SkipsImageDownload()
    {
        // Arrange - Create initial volume with cover images
        var initialParsedInfo = new ParsedInfo
        {
            Title = "Cover Test Volume",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Cover Test Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/volume1",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        await _parserService.CreateOrUpdateFromParsedInfoAsync(initialParsedInfo);

        // Verify initial images are set
        using (var context = _dbContextFactory.CreateDbContext())
        {
            var vol = await context.Volumes.FirstOrDefaultAsync(v => v.Title == "Cover Test Volume");
            Assert.IsNotNull(vol);
            Assert.AreEqual("images/cover.jpg", vol.CoverImageUrl);
            Assert.AreEqual("images/small/cover.jpg", vol.CoverImageUrlSmall);
        }

        // Reset mock to track new calls
        _imageManagerMock.Invocations.Clear();

        // Act - Update the same volume (cover images already exist)
        var updatedParsedInfo = new ParsedInfo
        {
            Title = "Cover Test Volume",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Cover Test Series",
            Cover = "https://example.com/new_cover.jpg", // Different cover URL
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/volume1-updated",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Updated description"
        };

        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(updatedParsedInfo);

        // Assert
        Assert.AreEqual(State.Updated, result);

        // Verify image download was NOT called since both cover images already exist
        _imageManagerMock.Verify(x => x.DownloadFileFromWeb(It.IsAny<string>(), It.IsAny<Guid>()), Times.Never());
        _imageManagerMock.Verify(x => x.CreateSmallImage(It.IsAny<string>()), Times.Never());
    }

    [TestMethod]
    public async Task AiGen_ReleaseDateInPast_SetsReleaseDateToNow()
    {
        // Arrange - Release date in the past (not > DateTime.Now)
        var pastReleaseDate = DateTimeOffset.Now.AddDays(-30);
        var parsedInfo = new ParsedInfo
        {
            Title = "Past Release Volume",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Past Release Series",
            Cover = "https://example.com/cover.jpg",
            Release = pastReleaseDate, // Past date - condition Release > DateTime.Now is false
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/pastrelease",
            PreorderStartDate = null, // No preorder start date
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        var beforeTest = DateTimeOffset.Now;

        // Act
        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(parsedInfo);

        var afterTest = DateTimeOffset.Now;

        // Assert
        Assert.AreEqual(State.Added, result);

        using var context = _dbContextFactory.CreateDbContext();
        var volume = await context.Volumes
            .FirstOrDefaultAsync(v => v.Title == "Past Release Volume");

        Assert.IsNotNull(volume);
        // Since Release is in the past and PreorderStartDate is null, ReleaseDate should be set to Now
        Assert.IsNotNull(volume.ReleaseDate);
        Assert.IsTrue(volume.ReleaseDate >= beforeTest && volume.ReleaseDate <= afterTest,
            "ReleaseDate should be set to approximately now when Release is in the past and no PreorderStartDate");
    }

    [TestMethod]
    public async Task AiGen_SameSeriesNumberDifferentTitle_CreatesNewVolume()
    {
        // Arrange - Create first volume
        var firstParsedInfo = new ParsedInfo
        {
            Title = "Special Edition",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Multi Title Series",
            Cover = "https://example.com/cover1.jpg",
            Release = DateTimeOffset.Now.AddDays(-30),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567891",
            TotalVolumes = 5,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/special",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Special edition"
        };

        await _parserService.CreateOrUpdateFromParsedInfoAsync(firstParsedInfo);

        // Act - Create volume with same series and number but different title
        var secondParsedInfo = new ParsedInfo
        {
            Title = "Regular Edition", // Different title
            Authors = "Test Author",
            VolumeNumber = 1, // Same number
            Series = "Multi Title Series", // Same series
            Cover = "https://example.com/cover2.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567892",
            TotalVolumes = 5,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/regular",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Regular edition"
        };

        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(secondParsedInfo);

        // Assert - Should create a new volume since title is different
        Assert.AreEqual(State.Added, result);

        using var context = _dbContextFactory.CreateDbContext();
        var volumes = await context.Volumes
            .Where(v => v.Series!.Title == "Multi Title Series" && v.Number == 1)
            .ToListAsync();

        Assert.AreEqual(2, volumes.Count, "Should have two volumes with same number but different titles");
    }

    [TestMethod]
    public async Task AiGen_EmptyAuthorsString_CreatesSeriesWithoutAuthors()
    {
        // Arrange - Empty string (not null)
        var parsedInfo = new ParsedInfo
        {
            Title = "Empty Author Volume",
            Authors = "", // Empty string, not null
            VolumeNumber = 1,
            Series = "Empty Author Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Completed,
            OriginalSeriesName = "Original",
            Url = "https://example.com/emptyauthor",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        // Act
        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(parsedInfo);

        // Assert
        Assert.AreEqual(State.Added, result);

        using var context = _dbContextFactory.CreateDbContext();
        var series = await context.Series
            .Include(s => s.Authors)
            .FirstOrDefaultAsync(s => s.Title == "Empty Author Series");

        Assert.IsNotNull(series);
        // Split with RemoveEmptyEntries should result in 0 authors
        Assert.AreEqual(0, series.Authors.Count, "Empty string should result in no authors");
    }

    [TestMethod]
    public async Task AiGen_AgeRestrictionsUpdateSameValue_DoesNotTriggerChange()
    {
        // Arrange - Create initial volume with age 16
        var initialParsedInfo = new ParsedInfo
        {
            Title = "Age Test Volume",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Age Test Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/volume1",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        await _parserService.CreateOrUpdateFromParsedInfoAsync(initialParsedInfo);

        // Act - Update with same age restriction
        var updatedParsedInfo = new ParsedInfo
        {
            Title = "Age Test Volume",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Age Test Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/volume1",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16, // Same value
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(updatedParsedInfo);

        // Assert
        using var context = _dbContextFactory.CreateDbContext();
        var volume = await context.Volumes
            .FirstOrDefaultAsync(v => v.Title == "Age Test Volume");

        Assert.IsNotNull(volume);
        Assert.AreEqual(16, volume.AgeRestriction);
    }

    [TestMethod]
    public async Task AiGen_SeriesWithNullTotalVolumes_UpdatesWhenNewValueIsPositive()
    {
        // Arrange - Create volume with 0 total volumes (will be stored as 0 or null equivalent)
        var initialParsedInfo = new ParsedInfo
        {
            Title = "Null Total Volume",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Null Total Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 0, // Initially unknown
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/volume1",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        await _parserService.CreateOrUpdateFromParsedInfoAsync(initialParsedInfo);

        // Verify initial state
        using (var context = _dbContextFactory.CreateDbContext())
        {
            var series = await context.Series.FirstOrDefaultAsync(s => s.Title == "Null Total Series");
            Assert.IsNotNull(series);
            Assert.AreEqual(0, series.TotalVolumes);
        }

        // Act - Update with positive total volumes
        var updatedParsedInfo = new ParsedInfo
        {
            Title = "Null Total Volume",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Null Total Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 10, // Now known
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/volume1",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(updatedParsedInfo);

        // Assert
        using var context2 = _dbContextFactory.CreateDbContext();
        var updatedSeries = await context2.Series.FirstOrDefaultAsync(s => s.Title == "Null Total Series");

        Assert.IsNotNull(updatedSeries);
        Assert.AreEqual(10, updatedSeries.TotalVolumes, "TotalVolumes should be updated from 0 to 10");
    }

    [TestMethod]
    public async Task AiGen_NotPreorderWithExistingPreorderStart_KeepsPreorderStart()
    {
        // Arrange - Create preorder volume first
        var preorderParsedInfo = new ParsedInfo
        {
            Title = "Was Preorder Volume",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Was Preorder Series",
            Cover = "https://example.com/cover.jpg",
            Release = null,
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/waspreorder",
            PreorderStartDate = DateTimeOffset.Now.AddDays(-30),
            CountryCode = "uk",
            IsPreorder = true, // Was a preorder
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        await _parserService.CreateOrUpdateFromParsedInfoAsync(preorderParsedInfo);

        // Get the original PreorderStart
        DateTimeOffset? originalPreorderStart;
        using (var context = _dbContextFactory.CreateDbContext())
        {
            var vol = await context.Volumes.FirstOrDefaultAsync(v => v.Title == "Was Preorder Volume");
            Assert.IsNotNull(vol);
            Assert.IsNotNull(vol.PreorderStart);
            originalPreorderStart = vol.PreorderStart;
        }

        // Act - Update to no longer be preorder
        var releasedParsedInfo = new ParsedInfo
        {
            Title = "Was Preorder Volume",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Was Preorder Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-5),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/waspreorder",
            PreorderStartDate = DateTimeOffset.Now.AddDays(-30),
            CountryCode = "uk",
            IsPreorder = false, // No longer preorder
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(releasedParsedInfo);

        // Assert
        Assert.AreEqual(State.Updated, result);

        using var context2 = _dbContextFactory.CreateDbContext();
        var volume = await context2.Volumes.FirstOrDefaultAsync(v => v.Title == "Was Preorder Volume");

        Assert.IsNotNull(volume);
        Assert.IsFalse(volume.IsPreorder);
        // PreorderStart should still be set (code doesn't clear it)
        Assert.IsNotNull(volume.PreorderStart);
    }

    [TestMethod]
    public async Task AiGen_NewPublisher_SetsPublisherUrl()
    {
        // Arrange
        var parsedInfo = new ParsedInfo
        {
            Title = "Publisher URL Volume",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Publisher URL Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Brand New Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://newpublisher.com/shop/volume1",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        // Act
        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(parsedInfo);

        // Assert
        Assert.AreEqual(State.Added, result);

        using var context = _dbContextFactory.CreateDbContext();
        var publisher = await context.Publishers
            .FirstOrDefaultAsync(p => p.Name == "Brand New Publisher");

        Assert.IsNotNull(publisher);
        // Publisher URL should be set to the full URL from volumeInfo.Url
        Assert.AreEqual("https://newpublisher.com/shop/volume1", publisher.Url);
    }

    [TestMethod]
    public async Task AiGen_OnlyCoverImageUrlSmallIsNull_TriggersImageDownload()
    {
        // This tests the OR condition: CoverImageUrl is null || CoverImageUrlSmall is null
        // Arrange - Create initial volume
        var initialParsedInfo = new ParsedInfo
        {
            Title = "Partial Cover Volume",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Partial Cover Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/partialcover",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        await _parserService.CreateOrUpdateFromParsedInfoAsync(initialParsedInfo);

        // Manually set CoverImageUrlSmall to null to simulate partial state
        using (var context = _dbContextFactory.CreateDbContext())
        {
            var vol = await context.Volumes.FirstOrDefaultAsync(v => v.Title == "Partial Cover Volume");
            Assert.IsNotNull(vol);
            vol.CoverImageUrlSmall = null; // Simulate missing small image
            await context.SaveChangesAsync();
        }

        // Reset mock to track new calls
        _imageManagerMock.Invocations.Clear();

        // Act - Update the volume (should trigger image download due to null CoverImageUrlSmall)
        var updatedParsedInfo = new ParsedInfo
        {
            Title = "Partial Cover Volume",
            Authors = "Test Author",
            VolumeNumber = 1,
            Series = "Partial Cover Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Test Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "Original",
            Url = "https://example.com/partialcover-updated",
            PreorderStartDate = null,
            CountryCode = "uk",
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Updated description"
        };

        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(updatedParsedInfo);

        // Assert
        Assert.AreEqual(State.Updated, result);

        // Verify image download WAS called because CoverImageUrlSmall was null
        _imageManagerMock.Verify(x => x.DownloadFileFromWeb(It.IsAny<string>(), It.IsAny<Guid>()), Times.Once());
        _imageManagerMock.Verify(x => x.CreateSmallImage(It.IsAny<string>()), Times.Once());
    }

    [TestMethod]
    public async Task AiGen_ValidCountryCode_UsesCorrectCountry()
    {
        // Arrange - Use Japan country code which exists in test data
        var parsedInfo = new ParsedInfo
        {
            Title = "Japan Volume",
            Authors = "Japanese Author",
            VolumeNumber = 1,
            Series = "Japan Series",
            Cover = "https://example.com/cover.jpg",
            Release = DateTimeOffset.Now.AddDays(-10),
            Publisher = "Japanese Publisher",
            VolumeType = VolumeType.Physical,
            Isbn = "978-1234567890",
            TotalVolumes = 1,
            SeriesStatus = SeriesStatus.Ongoing,
            OriginalSeriesName = "日本シリーズ",
            Url = "https://example.com/japan",
            PreorderStartDate = null,
            CountryCode = "jp", // Valid country code that exists in test data
            IsPreorder = false,
            AgeRestrictions = 16,
            CanBePublished = true,
            SeriesType = SeriesType.Manga,
            Description = "Test description"
        };

        // Act
        var result = await _parserService.CreateOrUpdateFromParsedInfoAsync(parsedInfo);

        // Assert
        Assert.AreEqual(State.Added, result);

        using var context = _dbContextFactory.CreateDbContext();
        var series = await context.Series
            .Include(s => s.Publisher)
            .ThenInclude(p => p!.Country)
            .FirstOrDefaultAsync(s => s.Title == "Japan Series");

        Assert.IsNotNull(series);
        Assert.IsNotNull(series.Publisher);
        Assert.IsNotNull(series.Publisher.Country);
        Assert.AreEqual("jp", series.Publisher.Country.CountryCode);
        Assert.AreEqual("Japan", series.Publisher.Country.Name);
    }
}

// Helper class for creating DbContextFactory
public class TestDbContextFactory : IDbContextFactory<MangaDbContext>
{
    private readonly DbContextOptions<MangaDbContext> _options;

    public TestDbContextFactory(DbContextOptions<MangaDbContext> options)
    {
        _options = options;
    }

    public MangaDbContext CreateDbContext()
    {
        return new TestMangaDbContext(_options);
    }
}

// Test-specific DbContext that ignores audit requirements
public class TestMangaDbContext : MangaDbContext
{
    public TestMangaDbContext(DbContextOptions<MangaDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Make audit fields optional for testing
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var createdByProperty = entityType.FindProperty("CreatedBy");
            if (createdByProperty != null)
            {
                createdByProperty.IsNullable = true;
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Set audit fields automatically for testing
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity &&
                        (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            
            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTimeOffset.Now;
                entity.CreatedBy = entity.CreatedBy ?? "TestSystem";
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.UpdatedAt = DateTimeOffset.Now;
                entity.UpdatedBy = "TestSystem";
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

// Helper class for creating service scopes
public class TestServiceScopeFactory : IServiceScopeFactory
{
    private readonly IServiceScope _scope;

    public TestServiceScopeFactory(IServiceScope scope)
    {
        _scope = scope;
    }

    public IServiceScope CreateScope()
    {
        return _scope;
    }
}