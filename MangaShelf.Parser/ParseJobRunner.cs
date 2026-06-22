using MangaShelf.BL.Configuration;
using MangaShelf.BL.Contracts;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace MangaShelf.Parser;

internal class ParseJobRunner : IParseJobRunner, IDisposable
{
    private readonly object locker = new();
    private SemaphoreSlim _semaphore;
    private readonly ILogger<ParseJobRunner> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly JobManagerSettings _options;

    private readonly ConcurrentQueue<Guid> queue = new();
    private readonly ConcurrentDictionary<Guid, byte> queuedJobs = new();
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private Task queueTask;

    public ParseJobRunner(
        ILogger<ParseJobRunner> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        using (var scope = _serviceProvider.CreateScope())
        {
            _options = scope.ServiceProvider.GetRequiredService<IConfigurationService>().JobManager;
        }

        runQueueHandler();
    }

    private void runQueueHandler()
    {
        _semaphore = new SemaphoreSlim(_options.MaxParallelParsers, _options.MaxParallelParsers);
        var runningTasks = new List<Task>();

        queueTask = Task.Run(async () =>
        {
            try
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    lock (runningTasks)
                    {
                        runningTasks.RemoveAll(t => t.IsCompleted);
                    }

                    if (queue.TryDequeue(out var jobId))
                    {
                        queuedJobs.TryRemove(jobId, out _);
                        await _semaphore.WaitAsync(cancellationTokenSource.Token);

                        var jobTask = Task.Run(async () =>
                        {
                            try
                            {
                                using var scope = _serviceProvider.CreateScope();
                                var parseService = scope.ServiceProvider.GetRequiredService<IParseService>();
                                await parseService.RunParseJob(jobId, cancellationTokenSource.Token);
                            }
                            catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
                            {
                                _logger.LogDebug("Job {JobId} cancelled", jobId);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error running job {JobId}", jobId);
                            }
                            finally
                            {
                                _semaphore.Release();
                            }
                        });

                        lock (runningTasks)
                        {
                            runningTasks.Add(jobTask);
                        }

                        await Task.Delay(100, cancellationTokenSource.Token);
                    }
                    else
                    {
                        await Task.Delay(5000, cancellationTokenSource.Token);
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
            {
                _logger.LogDebug("Queue handler cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in runQueueHandler loop");
            }
        }, cancellationTokenSource.Token);
    }

    public async Task RunJobs(CancellationToken token)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var statusService = scope.ServiceProvider.GetRequiredService<IParseJobManagerService>();
            try
            {
                if (_options.ScheduledJobsEnabled)
                {
                    await statusService.CreateScheduledJobs(token);
                }

                var jobs = await statusService.PrepareWaitingJobs(token);

                foreach (var item in jobs)
                {
                    if (!queuedJobs.TryAdd(item, 0))
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

    public void Dispose()
    {
        lock (locker)
        {
            if (!cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource.Cancel();
            }

            if (queueTask != null)
            {
                try
                {
                    queueTask.Wait();
                }
                catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is TaskCanceledException || e is OperationCanceledException))
                {
                }

                queueTask.Dispose();
                queueTask = null;
            }

            cancellationTokenSource.Dispose();

            if (_semaphore != null)
            {
                _semaphore.Dispose();
            }
        }
    }

    public async Task InitializeParsers(CancellationToken token)
    {
        using var scope = _serviceProvider.CreateScope();
        var parserFactory = scope.ServiceProvider.GetRequiredService<IParserFactory>();
        var parsers = parserFactory.GetParsers();

        _logger.LogInformation("Found {ParserCount} parsers", parsers.Count());

        var jobManagerService = scope.ServiceProvider.GetRequiredService<IParseJobManagerService>();
        await jobManagerService.InitializeParsers(parsers.Select(x => x.ParserName), token);
    }
}

public interface IParseJobRunner
{
    Task InitializeParsers(CancellationToken token);
    Task RunJobs(CancellationToken token);
}