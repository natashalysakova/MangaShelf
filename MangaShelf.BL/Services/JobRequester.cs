using MangaShelf.BL.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MangaShelf.BL.Services;

public class JobRequester : IJobRequester
{
    private readonly IServiceProvider _serviceProvider;
    public JobRequester(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Run parser for one url
    /// </summary>
    /// <param name="url"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<Guid> CreateSingleJob(string url, CancellationToken token = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IParserFactory>();
        var parser = factory.GetParsers().SingleOrDefault(p => p.CanParse(url));
        if (parser == null)
        {
            throw new InvalidOperationException($"Url {url} does not supported");
        }

        var parserService = scope.ServiceProvider.GetRequiredService<IParserWriteService>();
        var jobId = await parserService.CreateSingleJob(parser.ParserName, url, token);

        return jobId;
    }

    /// <summary>
    /// Run one parser separately
    /// </summary>
    /// <param name="parserId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<Guid> CreateParserJob(Guid parserId, CancellationToken token = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IParserFactory>();

        var parserService = scope.ServiceProvider.GetRequiredService<IParserWriteService>();
        var jobId = await parserService.CreateParserJob(parserId, token);

        return jobId;
    }

    public async Task CancelJob(Guid jobId, CancellationToken token = default)
    {
        var scope = _serviceProvider.CreateScope();
        var parserService = scope.ServiceProvider.GetRequiredService<IParserWriteService>();
        await parserService.CancelJob(jobId, token);
    }
}
