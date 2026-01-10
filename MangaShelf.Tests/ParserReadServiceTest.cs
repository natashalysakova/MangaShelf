using Microsoft.EntityFrameworkCore;
using MangaShelf.BL.Services.Parsing;
using MangaShelf.DAL.System;
using MangaShelf.DAL.System.Models;
using Xunit;
using Assert = Xunit.Assert;

namespace MangaShelf.Tests;

public class ParserReadServiceTest : IDisposable
{
    private readonly string _databaseName;
    private readonly DbContextOptions<MangaSystemDbContext> _options;
    private readonly IDbContextFactory<MangaSystemDbContext> _dbContextFactory;
    private readonly ParserReadService _service;

    public ParserReadServiceTest()
    {
        // Use a unique database name for each test instance
        // Important: All contexts must share the same options instance for in-memory database to work
        _databaseName = Guid.NewGuid().ToString();
        
        _options = new DbContextOptionsBuilder<MangaSystemDbContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .EnableSensitiveDataLogging()
            .Options;

        var factory = new TestDbContextFactory(_options);
        _dbContextFactory = factory;
        _service = new ParserReadService(_dbContextFactory);
    }

    private MangaSystemDbContext CreateContext()
    {
        return new MangaSystemDbContext(_options);
    }

    [Fact]
    public async Task GetJobById_ExistingJob_ReturnsJob()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var parser = new Parser { Id = Guid.NewGuid(), ParserName = "TestParser" };
        var job = new ParserJob 
        { 
            Id = jobId, 
            Status = RunStatus.Running,
            ParserStatusId = parser.Id,
            ParserStatus = parser
        };
        
        using (var context = CreateContext())
        {
            context.Parsers.Add(parser);
            context.Runs.Add(job);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await _service.GetJobById(jobId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(jobId, result.Id);
    }

    [Fact]
    public async Task GetJobById_NonExistingJob_ReturnsNull()
    {
        // Arrange
        var jobId = Guid.NewGuid();

        // Act
        var result = await _service.GetJobById(jobId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetJobs_ReturnsJobsInDescendingOrder()
    {
        // Arrange
        var parser = new Parser { Id = Guid.NewGuid(), ParserName = "TestParser" };
        var job1 = new ParserJob 
        { 
            Id = Guid.NewGuid(), 
            Created = DateTime.UtcNow.AddDays(-1),
            ParserStatusId = parser.Id,
            ParserStatus = parser
        };
        var job2 = new ParserJob 
        { 
            Id = Guid.NewGuid(), 
            Created = DateTime.UtcNow,
            ParserStatusId = parser.Id,
            ParserStatus = parser
        };
        
        using (var context = CreateContext())
        {
            context.Parsers.Add(parser);
            context.Runs.AddRange(job1, job2);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await _service.GetJobs();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Equal(job2.Id, result.First().Id);
    }

    [Fact]
    public async Task GetJobStatusById_ExistingJob_ReturnsStatus()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var job = new ParserJob { Id = jobId, Status = RunStatus.Finished };
        
        using (var context = CreateContext())
        {
            context.Runs.Add(job);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await _service.GetJobStatusById(jobId, CancellationToken.None);

        // Assert
        Assert.Equal(RunStatus.Finished, result);
    }

    [Fact]
    public async Task GetJobStatusById_NonExistingJob_ThrowsException()
    {
        // Arrange
        var jobId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.GetJobStatusById(jobId, CancellationToken.None));
    }

    [Fact]
    public async Task GetNewJobsSince_ReturnsJobsAfterDateTime()
    {
        // Arrange
        var cutoffTime = DateTimeOffset.UtcNow.AddHours(-1);
        var parser = new Parser { Id = Guid.NewGuid(), ParserName = "TestParser" };
        var oldJob = new ParserJob 
        { 
            Id = Guid.NewGuid(), 
            Started = cutoffTime.AddHours(-2),
            ParserStatusId = parser.Id,
            ParserStatus = parser
        };
        var newJob = new ParserJob 
        { 
            Id = Guid.NewGuid(), 
            Started = cutoffTime.AddMinutes(30),
            ParserStatusId = parser.Id,
            ParserStatus = parser
        };
        
        using (var context = CreateContext())
        {
            context.Parsers.Add(parser);
            context.Runs.AddRange(oldJob, newJob);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await _service.GetNewJobsSince(cutoffTime);

        // Assert
        Assert.Single(result);
        Assert.Equal(newJob.Id, result.First().Id);
    }

    public void Dispose()
    {
        // Clean up the in-memory database
        using var context = CreateContext();
        context.Database.EnsureDeleted();
    }

    private class TestDbContextFactory : IDbContextFactory<MangaSystemDbContext>
    {
        private readonly DbContextOptions<MangaSystemDbContext> _options;

        public TestDbContextFactory(DbContextOptions<MangaSystemDbContext> options)
        {
            _options = options;
        }

        public MangaSystemDbContext CreateDbContext()
        {
            return new MangaSystemDbContext(_options);
        }
    }
}