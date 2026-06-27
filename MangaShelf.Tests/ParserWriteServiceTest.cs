using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MangaShelf.DAL.System;
using MangaShelf.DAL.System.Models;
using Xunit;
using Assert = Xunit.Assert;

using ParserModel = MangaShelf.DAL.System.Models.Parser;
using MangaShelf.DAL.Models;
using MangaShelf.Common.Interfaces;
using MangaShelf.BL.Contracts;
using MangaShelf.BL.Services.Parsing;
using Moq;
using Microsoft.Extensions.Logging;

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

            var serviceProvider = services.BuildServiceProvider();
            _dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<MangaSystemDbContext>>();
            var logger = new Mock<ILogger<ParseJobManagerService>>().Object;
            var configuration = new Mock<IConfigurationService>().Object;
            _service = new ParseJobManagerService(_dbContextFactory, configuration, logger);
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
            var job = new ParserJob { Id = Guid.NewGuid(), Progress = 0, ParserStatus = parser };
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