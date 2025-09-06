using MangaShelf.BL.Dto;
using MangaShelf.BL.Interfaces;
using MangaShelf.BL.Mappers;
using MangaShelf.DAL.System;
using MangaShelf.DAL.System.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.BL.Services.Parsing;

public class ParserReadService(IDbContextFactory<MangaSystemDbContext> dbContextFactory) : IParserReadService
{
    public async Task<ParserJob?> GetJobById(Guid jobId, CancellationToken token = default)
    {
        using var contex = dbContextFactory.CreateDbContext();
        var run = await contex.Runs
            .Include(p => p.ParserStatus)
            .Include(r => r.Errors)
            .FirstOrDefaultAsync(r => r.Id == jobId);

        return run;
    }

    public async Task<IEnumerable<ParserJob>> GetJobs(int count = 100, CancellationToken cancellationToken = default)
    {
        using var contex = dbContextFactory.CreateDbContext();

        var lastRuns = contex.Runs
            .Include(p => p.ParserStatus)
            .Include(r => r.Errors)
            .OrderByDescending(r => r.Created)
            .Take(count);

        return await lastRuns.ToListAsync(cancellationToken);
    }

    public async Task<RunStatus> GetJobStatusById(Guid jobId, CancellationToken token)
    {
        using var contex = dbContextFactory.CreateDbContext();
        var job = await contex.Runs
            .SingleOrDefaultAsync(r => r.Id == jobId, token);

        if (job == null)
        {
            throw new Exception($"No job found with id {jobId}");
        }
        return job.Status;
    }

    public async Task<IEnumerable<ParserJob>> GetNewJobsSince(DateTimeOffset dateTime, CancellationToken token = default)
    {
        using var contex = dbContextFactory.CreateDbContext();

        var newRuns = contex.Runs
            .Include(p => p.ParserStatus)
            .Include(r => r.Errors)
            .Where(r => r.Started >= dateTime)
            .OrderByDescending(r => r.Started);

        return await newRuns.ToListAsync(token);
    }

    public async Task<IEnumerable<ParserStatusDto>> GetStatusesAsync(CancellationToken token = default)
    {
        using var context = dbContextFactory.CreateDbContext();
        List<ParserStatusDto> parserStatusDtos = new();

        var parsers = await context.Parsers
            .Include(p => p.Jobs.OrderBy(r => r.Created).Where(x=>x.Status == RunStatus.Running))
                .ThenInclude(r => r.Errors)
            .ToListAsync(token);

        foreach (var parser in parsers)
        {
            parserStatusDtos.Add(parser.ToStatusDto());
        }

        return parserStatusDtos;
    }
}
