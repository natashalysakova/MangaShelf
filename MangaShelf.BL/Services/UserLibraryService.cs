using MangaShelf.BL.Dto;
using MangaShelf.BL.Mappers;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL;
using MangaShelf.DAL.DomainServices;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static MangaShelf.BL.Services.UserLibraryService;

namespace MangaShelf.BL.Services;

public interface IUserLibraryService
{
    Task<UserLibraryDto> GetUserPreorders(string userId, IFilterOptions? paginationOptions = default, CancellationToken cancellationToken = default);
    Task<UserLibraryDto> GetUserLibrary(string userId, IFilterOptions? paginationOptions = default, CancellationToken cancellationToken = default);
}
public class UserLibraryService : IUserLibraryService
{
    private readonly ILogger<UserLibraryService> _logger;
    private readonly IDbContextFactory<MangaDbContext> _dbContextFactory;

    public UserLibraryService(ILogger<UserLibraryService> logger, IDbContextFactory<MangaDbContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<UserLibraryDto> GetUserLibrary(string userId, IFilterOptions? paginationOptions = null, CancellationToken cancellationToken = default)
    {
        var dbContext = _dbContextFactory.CreateDbContext();

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.IdentityUserId == userId, cancellationToken);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        throw new NotImplementedException();
    }

    public async Task<UserLibraryDto> GetUserPreorders(string userId, IFilterOptions? paginationOptions = null, CancellationToken cancellationToken = default)
    {
        var dbContext = _dbContextFactory.CreateDbContext();

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.IdentityUserId == userId, cancellationToken);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        var userOwerships = await dbContext.Ownerships.Where(x => x.UserId == user.Id).ToListAsync(cancellationToken);
         
        var latestPreorderIds = userOwerships
            .GroupBy(o => o.VolumeId)
            .Select(v => v.OrderByDescending(x => x.Date).First())
            .Where(o => o.Status == VolumeStatus.Preorder)
            .Select(x => x.Id);

        var result = await dbContext.Ownerships
            .Include(x=>x.Volume)
                .ThenInclude(v => v.Series)
            .Include(x => x.Volume)
                .ThenInclude(v => v.History)
            .Where(x=> latestPreorderIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
        
        var everPreordered = await dbContext.Ownerships
            .Include(x => x.Volume)
                .ThenInclude(v => v.Series)
            .Include(x => x.Volume)
                .ThenInclude(v => v.History)
            .Where(x => x.UserId == user.Id && x.Status == VolumeStatus.Preorder)
            .ToListAsync(cancellationToken);
        var histories = everPreordered.SelectMany(x => x.Volume.History).ToList();

        return new UserLibraryDto()
        {
            UserId = userId,
            Volumes = result.Select(o => o.ToUserLibraryItemDto()).OrderBy(x => x.ReleaseDate),
            Updates = histories.Select(h => h.ToDto()).OrderByDescending(x => x.Date)
        };
    }
}

public class UserLibraryDto
{
    public string UserId { get; set; }

    public IEnumerable<UserLibraryItem> Volumes { get; set; }
    public IEnumerable<VolumeHistoryDto> Updates { get; set; }
}

public class UserLibraryItem
{
    public Guid VolumeId { get; set; }
    public string Title { get; set; }

    public VolumeStatus VolumeStatus { get; set; }
    public DateTimeOffset? ReleaseDate { get; set; }
    public int DaysTillRelease { get; set; }
    public bool IsPreorderDue { get => VolumeStatus == VolumeStatus.Preorder && ReleaseDate < DateTimeOffset.UtcNow; }

    public string? CoverUrl { get; set; } = null;
    
}

