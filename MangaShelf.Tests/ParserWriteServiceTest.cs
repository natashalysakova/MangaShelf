using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MangaShelf.BL.Services.Parsing;
using MangaShelf.DAL.System;
using MangaShelf.DAL.System.Models;
using Xunit;
using Assert = Xunit.Assert;

namespace MangaShelf.Tests
{
    public class ParserWriteServiceTest : IDisposable
    {
        private readonly IDbContextFactory<MangaSystemDbContext> _dbContextFactory;
        private readonly ParserWriteService _service;

        public ParserWriteServiceTest()
        {
            var services = new ServiceCollection();
            services.AddDbContextFactory<MangaSystemDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

            var serviceProvider = services.BuildServiceProvider();
            _dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<MangaSystemDbContext>>();
            _service = new ParserWriteService(_dbContextFactory);
        }

        [Fact]
        public async Task RecordError_WithValidRunId_AddsErrorToRun()
        {
            // Arrange
            using var context = _dbContextFactory.CreateDbContext();
            var run = new ParserJob { Id = Guid.NewGuid(), Status = RunStatus.Running };
            context.Runs.Add(run);
            await context.SaveChangesAsync();

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
            var exception = new Exception("Test exception");

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.RecordError(invalidId, "url", "{}", exception, CancellationToken.None));
        }

        [Fact]
        public async Task RunJob_WithValidJobId_UpdatesStatusAndStartTime()
        {
            // Arrange
            using var context = _dbContextFactory.CreateDbContext();
            var parser = new Parser { ParserName = "test", Status = ParserStatus.Idle };
            var job = new ParserJob { Id = Guid.NewGuid(), Status = RunStatus.Waiting, ParserStatus = parser };
            context.Parsers.Add(parser);
            context.Runs.Add(job);
            await context.SaveChangesAsync();

            // Act
            var result = await _service.RunJob(job.Id, CancellationToken.None);

            // Assert
            Assert.True(result);
            using var verifyContext = _dbContextFactory.CreateDbContext();
            var updatedJob = verifyContext.Runs.Include(r => r.ParserStatus).First(r => r.Id == job.Id);
            Assert.Equal(RunStatus.Running, updatedJob.Status);
            Assert.Equal(ParserStatus.GatheringVolumes, updatedJob.ParserStatus.Status);
            Assert.NotNull(updatedJob.Started);
        }

        [Fact]
        public async Task SetProgress_UpdatesProgressAndVolumes()
        {
            // Arrange
            using var context = _dbContextFactory.CreateDbContext();
            var parser = new Parser { ParserName = "test", Status = ParserStatus.Parsing };
            var job = new ParserJob { Id = Guid.NewGuid(), Progress = 0, ParserStatus = parser };
            context.Parsers.Add(parser);
            context.Runs.Add(job);
            await context.SaveChangesAsync();

            // Act
            await _service.SetProgress(job.Id, 50.0, "https://test.com", true, CancellationToken.None);

            // Assert
            using var verifyContext = _dbContextFactory.CreateDbContext();
            var updatedJob = verifyContext.Runs.First(r => r.Id == job.Id);
            Assert.Equal(50.0, updatedJob.Progress);
            Assert.Contains("https://test.com", updatedJob.VolumesUpdated);
        }

        [Fact]
        public async Task CreateSingleJob_CreatesJobWithCorrectType()
        {
            // Arrange
            using var context = _dbContextFactory.CreateDbContext();
            var parser = new Parser { ParserName = "test", Status = ParserStatus.Idle };
            context.Parsers.Add(parser);
            await context.SaveChangesAsync();

            // Act
            var jobId = await _service.CreateSingleJob("test", "https://test.com", CancellationToken.None);

            // Assert
            using var verifyContext = _dbContextFactory.CreateDbContext();
            var job = verifyContext.Runs.First(r => r.Id == jobId);
            Assert.Equal(ParserRunType.SingleUrl, job.Type);
            Assert.Equal("https://test.com", job.Url);
            Assert.Equal(RunStatus.Waiting, job.Status);
        }

        [Fact]
        public async Task CancelJob_UpdatesStatusToCancelled()
        {
            // Arrange
            using var context = _dbContextFactory.CreateDbContext();
            var parser = new Parser { ParserName = "test", Status = ParserStatus.Parsing };
            var job = new ParserJob { Id = Guid.NewGuid(), Status = RunStatus.Running, ParserStatus = parser };
            context.Runs.Add(job);
            await context.SaveChangesAsync();

            // Act
            await _service.CancelJob(job.Id, CancellationToken.None);

            // Assert
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
            // Arrange
            var parserNames = new[] { "parser1", "parser2" };

            // Act
            await _service.InitializeParsers(parserNames, false);

            // Assert
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
}