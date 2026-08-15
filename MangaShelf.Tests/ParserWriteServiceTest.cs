using MangaShelf.BL.Configuration;
using MangaShelf.BL.Contracts;
using MangaShelf.BL.Services.Parsing;
using MangaShelf.BL.Services.Parsing.Handlers;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;
using MangaShelf.DAL.System;
using MangaShelf.DAL.System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Assert = Xunit.Assert;
using ParserModel = MangaShelf.DAL.System.Models.Parser;

namespace MangaShelf.Tests
{
    public class ParserWriteServiceTest : IDisposable
    {
        private readonly IDbContextFactory<MangaSystemDbContext> _dbContextFactory;
        private readonly IParseJobManagerService _service;

        public ParserWriteServiceTest()
        {
            var services = new ServiceCollection();
            services.AddDbContextFactory<MangaSystemDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            services.AddLogging();

            services.AddScoped<IJobStateTransitionHandler, HandleJobErrorHandler>();
            services.AddScoped<IJobStateTransitionHandler, NotifyJobStatusChangedHandler>();
            services.AddScoped<IJobStateTransitionHandler, ParserStateHandler>();
            services.AddScoped<IJobStateTransitionHandler, ProgressChangeHandler>();
            services.AddScoped<IJobStateTransitionPublisher, JobStateTransitionPublisher>();

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
            var jobStateTransitoinPublisher = serviceProvider.GetRequiredService<IJobStateTransitionPublisher>();
            _service = new ParseJobManagerService(_dbContextFactory, configMock.Object, logger, jobStateTransitoinPublisher);
        }

        [Fact]
        public async Task RecordError_WithValidRunId_AddsErrorToRun()
        {
            // Arrange
            using var context = _dbContextFactory.CreateDbContext();
            var run = new ParserJob { Id = Guid.NewGuid(), Status = RunStatus.Running };
            context.Runs.Add(run);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var exception = new Exception("Test exception");
            var url = "https://test.com";
            var json = "{}";

            // Act
            await _service.RecordError(run.Id, url, json, exception, CancellationToken.None);

            // Assert
            using var verifyContext = _dbContextFactory.CreateDbContext();
            var updatedRun = verifyContext.Runs.Include(r => r.Errors).First(r => r.Id == run.Id);
            Assert.Single(updatedRun.Errors);
            Assert.Equal(url, updatedRun.Errors.First().Url);
            Assert.Equal(json, updatedRun.Errors.First().VolumeJson);
            Assert.Equal(exception.Message, updatedRun.Errors.First().ExceptionMessage);
        }

        [Fact]
        public async Task RecordError_WithInvalidRunId_ThrowsException()
        {
            // Arrange
            var invalidId = Guid.NewGuid();
            var exception = new InvalidOperationException("Test exception");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.RecordError(invalidId, "url", "{}", exception, CancellationToken.None));
        }

        [Fact]
        public async Task RunJob_WithValidJobId_UpdatesStatusAndStartTime()
        {
            // Arrange
            using var context = _dbContextFactory.CreateDbContext();
            var parser = new ParserModel { ParserName = "test", Status = ParserStatus.Idle };
            var job = new ParserJob { Id = Guid.NewGuid(), Status = RunStatus.Waiting, ParserStatus = parser, Type = ParserRunType.FullSite };
            context.Parsers.Add(parser);
            context.Runs.Add(job);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            await _service.RunJob(job.Id, CancellationToken.None);

            // Assert
            using var verifyContext = _dbContextFactory.CreateDbContext();
            var updatedJob = verifyContext.Runs.Include(r => r.ParserStatus).First(r => r.Id == job.Id);
            Assert.Equal(RunStatus.GatheringVolumes, updatedJob.Status);
            Assert.Equal(ParserStatus.GatheringVolumes, updatedJob.ParserStatus.Status);
            Assert.NotEqual(default, updatedJob.Started);
        }

        [Fact]
        public async Task SetProgress_UpdatesProgressAndVolumes()
        {
            // Arrange
            using var context = _dbContextFactory.CreateDbContext();
            var parser = new ParserModel { ParserName = "test", Status = ParserStatus.Parsing };
            var job = new ParserJob { Id = Guid.NewGuid(), Progress = 25, ParserStatus = parser, Status = RunStatus.Running };
            context.Parsers.Add(parser);
            context.Runs.Add(job);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var parseResult = new ParseResult(new ParseVolumeReference() { VolumeId = Guid.NewGuid(), FullName = "Test Volume", PublicId = "TestPublicId" }, State.Added);


            // Act
            await _service.SetProgress(job.Id, 50.0, parseResult, CancellationToken.None);

            // Assert
            using var verifyContext = _dbContextFactory.CreateDbContext();
            var updatedJob = verifyContext.Runs.Include(r => r.AddedVolumes).First(r => r.Id == job.Id);
            Assert.Equal(50.0, updatedJob.Progress);
            Assert.Contains(parseResult.VolumeReference.VolumeId, updatedJob.AddedVolumes.Select(v => v.VolumeId));
        }

        public void Dispose()
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.Database.EnsureDeleted();
        }
    }
}