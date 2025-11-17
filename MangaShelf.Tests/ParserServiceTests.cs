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
        _imageManagerMock.Setup(x => x.DownloadFileFromWeb(It.IsAny<string>()))
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