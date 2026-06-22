using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MangaShelf.BL.Configuration;
using MangaShelf.BL.Contracts;
using MangaShelf.BL.Services.Parsing;
using MangaShelf.DAL.System;
using MangaShelf.DAL.System.Models;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

using ParserModel = MangaShelf.DAL.System.Models.Parser;

namespace MangaShelf.Tests;

public class ParserJobManagerServiceTests : IDisposable
{
    private readonly IDbContextFactory<MangaSystemDbContext> _dbContextFactory;
    private readonly ParseJobManagerService _service;

    public ParserJobManagerServiceTests()
    {
        var services = new ServiceCollection();
        services.AddDbContextFactory<MangaSystemDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        var serviceProvider = services.BuildServiceProvider();
        _dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<MangaSystemDbContext>>();

        var configMock = new Mock<IConfigurationService>();
        configMock.Setup(x => x.JobManager).Returns(new JobManagerSettings
        {
            DelayBetweenRuns = TimeSpan.FromHours(1),
            MaxParallelParsers = 5,
            ResetNextRun = false,
            ScheduledJobsEnabled = true
        });

        _service = new ParseJobManagerService(_dbContextFactory, configMock.Object);
    }

    [Fact]
    public async Task CreateSingleJob_CreatesJobWithCorrectType()
    {
        using var context = _dbContextFactory.CreateDbContext();
        var parser = new ParserModel { ParserName = "test", Status = ParserStatus.Idle };
        context.Parsers.Add(parser);
        await context.SaveChangesAsync();

        var jobId = await _service.CreateSingleJob("test", "https://test.com", CancellationToken.None);

        using var verifyContext = _dbContextFactory.CreateDbContext();
        var job = verifyContext.Runs.First(r => r.Id == jobId);
        Assert.Equal(ParserRunType.SingleUrl, job.Type);
        Assert.Equal("https://test.com", job.Url);
        Assert.Equal(RunStatus.Waiting, job.Status);
    }

    [Fact]
    public async Task CancelJob_UpdatesStatusToCancelled()
    {
        using var context = _dbContextFactory.CreateDbContext();
        var parser = new ParserModel { ParserName = "test", Status = ParserStatus.Parsing };
        var job = new ParserJob { Id = Guid.NewGuid(), Status = RunStatus.Running, ParserStatus = parser };
        context.Runs.Add(job);
        await context.SaveChangesAsync();

        await _service.CancelJob(job.Id, CancellationToken.None);

        using var verifyContext = _dbContextFactory.CreateDbContext();
        var updatedJob = verifyContext.Runs.Include(r => r.ParserStatus).First(r => r.Id == job.Id);
        Assert.Equal(RunStatus.Cancelled, updatedJob.Status);
        Assert.Equal(ParserStatus.Idle, updatedJob.ParserStatus.Status);
        Assert.NotNull(updatedJob.Finished);
        Assert.Equal(-1, updatedJob.Progress);
    }

    [Fact]
    public async Task InitializeParsers_CreatesNewParsers()
    {
        var parserNames = new[] { "parser1", "parser2" };

        await _service.InitializeParsers(parserNames, CancellationToken.None);

        using var context = _dbContextFactory.CreateDbContext();
        var parsers = context.Parsers.ToList();
        Assert.Equal(2, parsers.Count);
        Assert.Contains(parsers, p => p.ParserName == "parser1");
        Assert.Contains(parsers, p => p.ParserName == "parser2");
    }

    public void Dispose()
    {
        using var context = _dbContextFactory.CreateDbContext();
        context.Database.EnsureDeleted();
    }
}
