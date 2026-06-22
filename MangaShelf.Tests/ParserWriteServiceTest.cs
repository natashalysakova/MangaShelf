using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MangaShelf.DAL.System;
using MangaShelf.DAL.System.Models;
using Xunit;
using Assert = Xunit.Assert;
using MangaShelf.Parser.Services;

using ParserModel = MangaShelf.DAL.System.Models.Parser;

namespace MangaShelf.Tests
{
    public class ParserWriteServiceTest : IDisposable
    {
        private readonly IDbContextFactory<MangaSystemDbContext> _dbContextFactory;
        private readonly ParserJobWiriterService _service;

        public ParserWriteServiceTest()
        {
            var services = new ServiceCollection();
            services.AddDbContextFactory<MangaSystemDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

            var serviceProvider = services.BuildServiceProvider();
            _dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<MangaSystemDbContext>>();
            _service = new ParserJobWiriterService(_dbContextFactory);
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
            var parser = new ParserModel { ParserName = "test", Status = ParserStatus.Idle };
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
            var parser = new ParserModel { ParserName = "test", Status = ParserStatus.Parsing };
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

        public void Dispose()
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.Database.EnsureDeleted();
        }
    }
}