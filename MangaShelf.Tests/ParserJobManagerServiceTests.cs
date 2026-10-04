using MangaShelf.BL.Configuration;
using MangaShelf.BL.Contracts;
using MangaShelf.BL.Services.Parsing;
using MangaShelf.BL.Services.Parsing.Handlers;
using MangaShelf.DAL.System;
using MangaShelf.DAL.System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

        services.AddLogging();
        services.AddScoped<IJobStateTransitionHandler, HandleJobErrorHandler>();
        services.AddScoped<IJobStateTransitionHandler, NotifyJobStatusChangedHandler>();
        services.AddScoped<IJobStateTransitionHandler, ParserStateHandler>();
        services.AddScoped<IJobStateTransitionHandler, ProgressChangeHandler>();

        var configMock = new Mock<IConfigurationService>();
        configMock.Setup(x => x.JobManager).Returns(new JobManagerSettings
        {
            DelayBetweenRuns = TimeSpan.FromHours(1),
            MaxParallelParsers = 5,
            ResetNextRun = false,
            ScheduledJobsEnabled = true
        });

        services.AddScoped<IConfigurationService>(provider => configMock.Object);

        var serviceProvider = services.BuildServiceProvider();
        _dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<MangaSystemDbContext>>();

        var logger = new Mock<ILogger<ParseJobManagerService>>().Object;

        var jobStateTransitoinPublisher = new JobStateTransitionPublisher(serviceProvider.GetServices<IJobStateTransitionHandler>(), new Mock<ILogger<JobStateTransitionPublisher>>().Object);
        _service = new ParseJobManagerService(_dbContextFactory, configMock.Object, logger, jobStateTransitoinPublisher);
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

    [Fact]
    public async Task DeleteOldJobs_RemovesFinishedAndCancelledJobsOlderThanCutoff_WhenRemoveFailedJobsIsFalse()
    {
        var parser = await AddParserAsync();
        var cutoffDate = DateTimeOffset.Now;

        await AddJobAsync(parser.Id, RunStatus.Finished, cutoffDate.AddDays(-2));
        await AddJobAsync(parser.Id, RunStatus.Cancelled, cutoffDate.AddDays(-2));
        await AddJobAsync(parser.Id, RunStatus.Error, cutoffDate.AddDays(-2));
        await AddJobAsync(parser.Id, RunStatus.Finished, cutoffDate.AddDays(2));

        var deletedCount = await _service.DeleteOldJobs(cutoffDate, removeFailedJobs: false, CancellationToken.None);

        Assert.Equal(2, deletedCount);

        using var context = _dbContextFactory.CreateDbContext();
        var remaining = context.Runs.ToList();
        Assert.Equal(2, remaining.Count);
        Assert.Contains(remaining, r => r.Status == RunStatus.Error);
        Assert.Contains(remaining, r => r.Status == RunStatus.Finished && r.Created > cutoffDate);
    }

    [Fact]
    public async Task DeleteOldJobs_RemovesErrorJobsToo_WhenRemoveFailedJobsIsTrue()
    {
        var parser = await AddParserAsync();
        var cutoffDate = DateTimeOffset.Now;

        await AddJobAsync(parser.Id, RunStatus.Finished, cutoffDate.AddDays(-2));
        await AddJobAsync(parser.Id, RunStatus.Cancelled, cutoffDate.AddDays(-2));
        await AddJobAsync(parser.Id, RunStatus.Error, cutoffDate.AddDays(-2));

        var deletedCount = await _service.DeleteOldJobs(cutoffDate, removeFailedJobs: true, CancellationToken.None);

        Assert.Equal(3, deletedCount);

        using var context = _dbContextFactory.CreateDbContext();
        Assert.Empty(context.Runs.ToList());
    }

    [Fact]
    public async Task DeleteOldJobs_DoesNotRemoveActiveJobs_RegardlessOfAge()
    {
        var parser = await AddParserAsync();
        var cutoffDate = DateTimeOffset.Now;

        await AddJobAsync(parser.Id, RunStatus.Running, cutoffDate.AddDays(-5));
        await AddJobAsync(parser.Id, RunStatus.Waiting, cutoffDate.AddDays(-5));
        await AddJobAsync(parser.Id, RunStatus.GatheringVolumes, cutoffDate.AddDays(-5));

        var deletedCount = await _service.DeleteOldJobs(cutoffDate, removeFailedJobs: true, CancellationToken.None);

        Assert.Equal(0, deletedCount);

        using var context = _dbContextFactory.CreateDbContext();
        Assert.Equal(3, context.Runs.ToList().Count);
    }

    [Fact]
    public async Task DeleteOldJobs_DoesNotRemoveJobsNewerThanCutoffDate()
    {
        var parser = await AddParserAsync();
        var cutoffDate = DateTimeOffset.Now;

        await AddJobAsync(parser.Id, RunStatus.Finished, cutoffDate.AddMinutes(1));
        await AddJobAsync(parser.Id, RunStatus.Cancelled, cutoffDate.AddMinutes(1));

        var deletedCount = await _service.DeleteOldJobs(cutoffDate, removeFailedJobs: true, CancellationToken.None);

        Assert.Equal(0, deletedCount);

        using var context = _dbContextFactory.CreateDbContext();
        Assert.Equal(2, context.Runs.ToList().Count);
    }

    [Fact]
    public async Task DeleteOldJobs_ReturnsZero_WhenNoJobsExist()
    {
        var deletedCount = await _service.DeleteOldJobs(DateTimeOffset.Now, removeFailedJobs: true, CancellationToken.None);

        Assert.Equal(0, deletedCount);
    }

    private async Task AddJobAsync(Guid parserId, RunStatus status, DateTimeOffset created)
    {
        using var context = _dbContextFactory.CreateDbContext();
        context.Runs.Add(new ParserJob
        {
            Id = Guid.NewGuid(),
            ParserStatusId = parserId,
            Status = status,
            Created = created,
        });
        await context.SaveChangesAsync();
    }

    private async Task<ParserModel> AddParserAsync(string name = "test")
    {
        using var context = _dbContextFactory.CreateDbContext();
        var parser = new ParserModel { ParserName = name, Status = ParserStatus.Idle };
        context.Parsers.Add(parser);
        await context.SaveChangesAsync();
        return parser;
    }

    public void Dispose()
    {
    }
}
