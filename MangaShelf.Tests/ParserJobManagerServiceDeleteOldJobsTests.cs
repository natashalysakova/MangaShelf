using MangaShelf.BL.Configuration;
using MangaShelf.BL.Contracts;
using MangaShelf.BL.Services.Parsing;
using MangaShelf.BL.Services.Parsing.Handlers;
using MangaShelf.DAL.System;
using MangaShelf.DAL.System.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Assert = Xunit.Assert;
using ParserModel = MangaShelf.DAL.System.Models.Parser;

namespace MangaShelf.Tests;

public class ParserJobManagerServiceDeleteOldJobsTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly IDbContextFactory<MangaSystemDbContext> _dbContextFactory;
    private readonly ParseJobManagerService _service;

    public ParserJobManagerServiceDeleteOldJobsTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddDbContextFactory<MangaSystemDbContext>(options =>
            options.UseSqlite(_connection));

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

        using var initContext = _dbContextFactory.CreateDbContext();
        initContext.Database.EnsureCreated();

        var logger = new Mock<ILogger<ParseJobManagerService>>().Object;
        var jobStateTransitionPublisher = new JobStateTransitionPublisher(
            serviceProvider.GetServices<IJobStateTransitionHandler>(),
            new Mock<ILogger<JobStateTransitionPublisher>>().Object);

        _service = new ParseJobManagerService(_dbContextFactory, configMock.Object, logger, jobStateTransitionPublisher);
    }






    public void Dispose()
    {
        _connection.Dispose();
    }
}
