using AngleSharp.Dom;
using MangaShelf.BL.Interfaces;
using MangaShelf.BL.Services.Parsing;
using MangaShelf.DAL.System;
using MangaShelf.DAL.System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace MangaShelf.BL.Services;

internal class ParseJobManger : IParseJobManager, IDisposable
{
    private readonly ILogger<ParseJobManger> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly JobManagerOptions _options;

    private readonly Queue<Guid> queue = new();
    private CancellationTokenSource cancellationTokenSource = new();
    private Task queueTask;

    public ParseJobManger(
        ILogger<ParseJobManger> logger,
        IServiceProvider serviceProvider,
        IOptions<JobManagerOptions> options)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _options = options.Value;

        runQueueHandler();
    }

    private void runQueueHandler()
    {
        // Using a semaphore to limit concurrent job executions to 5
        var semaphore = new SemaphoreSlim(_options.MaxParallelParsers, _options.MaxParallelParsers);
        var runningTasks = new List<Task>();

        queueTask = Task.Run(async () =>
        {
            while (true)
            {
                // Clean up completed tasks
                lock (runningTasks)
                {
                    runningTasks.RemoveAll(t => t.IsCompleted);
                }

                if (queue.TryDequeue(out var jobId))
                {
                    await semaphore.WaitAsync();

                    // Start a new task to process the job
                    var jobTask = Task.Run(async () =>
                    {
                        try
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var parseService = scope.ServiceProvider.GetRequiredService<IParseService>();
                            await parseService.RunParseJob(jobId, CancellationToken.None);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error running job {JobId}", jobId);
                        }
                        finally
                        {
                            // Release the semaphore when done
                            semaphore.Release();
                        }
                    });

                    // Track the running task
                    lock (runningTasks)
                    {
                        runningTasks.Add(jobTask);
                    }
                }
                else
                {
                    await Task.Delay(1000); // wait for 1 second before checking the queue again
                }
            }
        }, cancellationTokenSource.Token);
    }

    /// <summary>
    /// Create jobs that are scheduled to run
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<int> CreateScheduledJobs(CancellationToken token = default)
    {
        // get jobs from database that are scheduled to run
        using var scope = _serviceProvider.CreateScope();
        var parserService = scope.ServiceProvider.GetRequiredService<IParserWriteService>();
        return await parserService.CreateScheduledJobs(_options.DelayBetweenRuns);
    }



    public async Task RunScheduledJobs(CancellationToken token = default)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var statusService = scope.ServiceProvider.GetRequiredService<IParserWriteService>();
            var parseService = scope.ServiceProvider.GetRequiredService<IParseService>();
            try
            {
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = _options.MaxParallelParsers,
                    CancellationToken = token
                };

                var jobs = await statusService.PrepareWaitingJobs();

                foreach (var item in jobs)
                {
                    if(queue.Contains(item))
                    {
                        _logger.LogDebug("Job {JobId} is already in the queue, skipping", item);
                        continue;
                    }
                    queue.Enqueue(item);
                    _logger.LogDebug("Enqueued job {JobId}", item);
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in worker execution");
            }
        }
    }

    public async Task InitializeParser(IEnumerable<IPublisherParser> parsers)
    {
        using var scope = _serviceProvider.CreateScope();
        var parserService = scope.ServiceProvider.GetRequiredService<IParserWriteService>();
        await parserService.InitializeParsers(parsers.Select(x => x.ParserName), _options.ResetNextRun);
    }

    

    public void Dispose()
    {
       if(cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }
        if (queueTask != null)
        {
            try
            {
                queueTask.Wait();
            }
            catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is TaskCanceledException))
            {
                // Ignore TaskCanceledException
            }
            queueTask.Dispose();
            queueTask = null;
        }
    }
}
