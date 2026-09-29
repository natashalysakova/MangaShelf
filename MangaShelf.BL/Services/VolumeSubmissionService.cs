using MangaShelf.BL.Contracts;
using MangaShelf.BL.Dto;
using MangaShelf.Common.Helpers;
using MangaShelf.DAL;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.BL.Services;

public class VolumeSubmissionService(IDbContextFactory<MangaDbContext> dbContextFactory) : IVolumeSubmissionService
{
    public async Task SubmitAsync(VolumeSubmissionRequestDto request, string submittedByIdentityUserId, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(submittedByIdentityUserId))
        {
            throw new InvalidOperationException("A signed-in user is required to submit a volume.");
        }

        using var context = await dbContextFactory.CreateDbContextAsync(token);
        var series = await ResolveSeriesAsync(context, request, token);
        ValidateVolume(request);

        var normalizedTitle = NormalizeOptional(request.Title);
        var normalizedIsbn = VolumeHelper.NormalizedIsbn(request.ISBN);
        var duplicate = series != null && await context.Volumes
            .IgnoreQueryFilters()
            .AnyAsync(volume =>
                volume.SeriesId == series.Id &&
                ((volume.Number == request.Number && volume.Title == normalizedTitle) ||
                 (normalizedIsbn != null && volume.ISBN == normalizedIsbn)), token);

        if (duplicate)
        {
            throw new InvalidOperationException("This volume already exists in the selected series.");
        }

        var submission = new VolumeSubmission
        {
            SubmittedByIdentityUserId = submittedByIdentityUserId,
            CreatedBy = submittedByIdentityUserId,
            SeriesId = request.SeriesId,
            ExistingSeriesTitle = series?.Title,
            ExistingPublisherName = series?.Publisher?.Name,
            NewSeriesTitle = request.SeriesId.HasValue ? null : request.NewSeriesTitle!.Trim(),
            NewSeriesOriginalTitle = request.SeriesId.HasValue ? null : NormalizeOptional(request.NewSeriesOriginalTitle),
            NewSeriesType = request.NewSeriesType,
            NewSeriesStatus = request.NewSeriesStatus,
            NewSeriesTotalVolumes = request.NewSeriesTotalVolumes,
            PublisherId = request.SeriesId.HasValue ? null : request.PublisherId,
            NewPublisherName = request.SeriesId.HasValue ? null : NormalizeOptional(request.NewPublisherName),
            NewPublisherUrl = request.SeriesId.HasValue ? null : NormalizeWebUrl(request.NewPublisherUrl),
            NewPublisherCountryId = request.SeriesId.HasValue ? null : request.NewPublisherCountryId,
            Title = normalizedTitle,
            Number = request.Number,
            ISBN = normalizedIsbn,
            AgeRestriction = request.AgeRestriction,
            PurchaseUrl = NormalizeWebUrl(request.PurchaseUrl),
            Description = NormalizeOptional(request.Description),
            IsPreorder = request.IsPreorder,
            PreorderStart = request.PreorderStart,
            ReleaseDate = request.ReleaseDate,
            Type = request.Type,
            SingleIssue = request.SingleIssue
        };

        context.VolumeSubmissions.Add(submission);
        await context.SaveChangesAsync(token);
    }

    public async Task<IReadOnlyList<VolumeSubmissionDto>> GetPendingAsync(CancellationToken token = default)
    {
        using var context = await dbContextFactory.CreateDbContextAsync(token);
        var submissions = await context.VolumeSubmissions
            .AsNoTracking()
            .Where(x => x.Status == VolumeSubmissionStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(token);

        return submissions.Select(ToDto).ToList();
    }

    public async Task ApproveAsync(Guid submissionId, string reviewedByIdentityUserId, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(reviewedByIdentityUserId))
        {
            throw new InvalidOperationException("A signed-in administrator is required to review a submission.");
        }

        using var context = await dbContextFactory.CreateDbContextAsync(token);
        var submission = await context.VolumeSubmissions
            .FirstOrDefaultAsync(x => x.Id == submissionId, token)
            ?? throw new InvalidOperationException("Volume submission not found.");

        EnsurePending(submission);

        var series = submission.SeriesId.HasValue
            ? await context.Series.FirstOrDefaultAsync(x => x.Id == submission.SeriesId.Value, token)
            : null;
        if (submission.SeriesId.HasValue && series == null)
        {
            throw new InvalidOperationException("Selected series no longer exists.");
        }

        if (series == null)
        {
            var publisher = await ResolvePublisherAsync(context, submission, token);
            series = new Series
            {
                Title = submission.NewSeriesTitle!,
                OriginalTitle = submission.NewSeriesOriginalTitle,
                Type = submission.NewSeriesType,
                Status = submission.NewSeriesStatus,
                TotalVolumes = submission.NewSeriesTotalVolumes,
                IsPublishedOnSite = true,
                Publisher = publisher,
                CreatedBy = reviewedByIdentityUserId
            };
            context.Series.Add(series);
        }

        var normalizedIsbn = VolumeHelper.NormalizedIsbn(submission.ISBN);
        var duplicate = await context.Volumes
            .IgnoreQueryFilters()
            .AnyAsync(volume =>
                volume.SeriesId == series.Id &&
                ((volume.Number == submission.Number && volume.Title == submission.Title) ||
                 (normalizedIsbn != null && volume.ISBN == normalizedIsbn)), token);

        if (duplicate)
        {
            throw new InvalidOperationException("A volume with this number, title, or ISBN already exists in the selected series.");
        }

        var volume = new Volume
        {
            Title = submission.Title,
            Number = submission.Number,
            ISBN = normalizedIsbn,
            AgeRestriction = submission.AgeRestriction,
            PurchaseUrl = NormalizeWebUrl(submission.PurchaseUrl),
            Description = submission.Description,
            IsPreorder = submission.IsPreorder,
            PreorderStart = submission.PreorderStart,
            ReleaseDate = submission.ReleaseDate,
            Type = submission.Type,
            SingleIssue = submission.SingleIssue,
            Series = series,
            IsPublishedOnSite = true,
            CreatedBy = reviewedByIdentityUserId
        };

        context.Volumes.Add(volume);
        submission.Status = VolumeSubmissionStatus.Approved;
        submission.ApprovedVolumeId = volume.Id;
        submission.ReviewedByIdentityUserId = reviewedByIdentityUserId;
        submission.ReviewedAt = DateTimeOffset.UtcNow;
        submission.UpdatedBy = reviewedByIdentityUserId;
        await context.SaveChangesAsync(token);
    }

    public async Task RejectAsync(Guid submissionId, string reviewedByIdentityUserId, string? comment, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(reviewedByIdentityUserId))
        {
            throw new InvalidOperationException("A signed-in administrator is required to review a submission.");
        }

        using var context = await dbContextFactory.CreateDbContextAsync(token);
        var submission = await context.VolumeSubmissions
            .FirstOrDefaultAsync(x => x.Id == submissionId, token)
            ?? throw new InvalidOperationException("Volume submission not found.");

        EnsurePending(submission);
        submission.Status = VolumeSubmissionStatus.Rejected;
        submission.ReviewComment = NormalizeOptional(comment);
        submission.ReviewedByIdentityUserId = reviewedByIdentityUserId;
        submission.ReviewedAt = DateTimeOffset.UtcNow;
        submission.UpdatedBy = reviewedByIdentityUserId;
        await context.SaveChangesAsync(token);
    }

    private static async Task<Series?> ResolveSeriesAsync(MangaDbContext context, VolumeSubmissionRequestDto request, CancellationToken token)
    {
        if (request.SeriesId.HasValue)
        {
            if (request.PublisherId.HasValue || !string.IsNullOrWhiteSpace(request.NewPublisherName) ||
                !string.IsNullOrWhiteSpace(request.NewSeriesTitle))
            {
                throw new InvalidOperationException("Choose either an existing series or a new series.");
            }

            return await context.Series
                .Include(x => x.Publisher)
                .FirstOrDefaultAsync(x => x.Id == request.SeriesId.Value, token)
                ?? throw new InvalidOperationException("Selected series was not found.");
        }

        if (string.IsNullOrWhiteSpace(request.NewSeriesTitle))
        {
            throw new InvalidOperationException("Series title is required.");
        }

        var creatingPublisher = !string.IsNullOrWhiteSpace(request.NewPublisherName);
        if (creatingPublisher == request.PublisherId.HasValue)
        {
            throw new InvalidOperationException("Select an existing publisher or provide a new publisher.");
        }

        if (request.PublisherId.HasValue)
        {
            var publisherExists = await context.Publishers.AnyAsync(x => x.Id == request.PublisherId.Value, token);
            if (!publisherExists)
            {
                throw new InvalidOperationException("Selected publisher was not found.");
            }
        }
        else
        {
            if (!request.NewPublisherCountryId.HasValue ||
                !await context.Countries.AnyAsync(x => x.Id == request.NewPublisherCountryId.Value, token))
            {
                throw new InvalidOperationException("Select a valid country for the new publisher.");
            }

            var publisherName = request.NewPublisherName!.Trim();
            var publisherExists = await context.Publishers
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Name.ToLower() == publisherName.ToLower(), token);

            if (publisherExists)
            {
                throw new InvalidOperationException("A publisher with this name already exists. Select it instead.");
            }
        }

        if (!Enum.IsDefined(request.NewSeriesType) || !Enum.IsDefined(request.NewSeriesStatus))
        {
            throw new InvalidOperationException("Select a valid series type and status.");
        }

        if (request.NewSeriesTotalVolumes < 0)
        {
            throw new InvalidOperationException("The total volume count cannot be negative.");
        }

        return null;
    }

    private static async Task<Publisher> ResolvePublisherAsync(MangaDbContext context, VolumeSubmission submission, CancellationToken token)
    {
        if (submission.PublisherId.HasValue)
        {
            return await context.Publishers
                .FirstOrDefaultAsync(x => x.Id == submission.PublisherId.Value, token)
                ?? throw new InvalidOperationException("Selected publisher no longer exists.");
        }

        if (!submission.NewPublisherCountryId.HasValue)
        {
            throw new InvalidOperationException("Country is required for the new publisher.");
        }

        var countryExists = await context.Countries
            .AnyAsync(x => x.Id == submission.NewPublisherCountryId.Value, token);
        if (!countryExists)
        {
            throw new InvalidOperationException("Selected country no longer exists.");
        }

        var name = submission.NewPublisherName?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Publisher name is required.");
        }

        var duplicate = await context.Publishers
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Name.ToLower() == name.ToLower(), token);

        if (duplicate)
        {
            throw new InvalidOperationException("A publisher with this name already exists. Reject the submission or restore the existing publisher.");
        }

        var publisher = new Publisher
        {
            Name = name,
            Url = NormalizeWebUrl(submission.NewPublisherUrl),
            CountryId = submission.NewPublisherCountryId.Value
        };
        context.Publishers.Add(publisher);
        return publisher;
    }

    private static void ValidateVolume(VolumeSubmissionRequestDto request)
    {
        if (request.ReleaseDate == default)
        {
            throw new InvalidOperationException("Release date is required.");
        }

        if (request.Number < 0 || request.AgeRestriction < 0 || request.NewSeriesTotalVolumes < 0)
        {
            throw new InvalidOperationException("Volume numbers, age restrictions, and total volume counts cannot be negative.");
        }

        if (!Enum.IsDefined(request.Type))
        {
            throw new InvalidOperationException("Select a valid volume type.");
        }
    }

    private static void EnsurePending(VolumeSubmission submission)
    {
        if (submission.Status != VolumeSubmissionStatus.Pending)
        {
            throw new InvalidOperationException("This submission has already been reviewed.");
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeWebUrl(string? value)
    {
        var normalized = NormalizeOptional(value);
        if (normalized == null)
        {
            return null;
        }

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("URLs must use http or https.");
        }

        return normalized;
    }

    private static VolumeSubmissionDto ToDto(VolumeSubmission submission)
    {
        return new VolumeSubmissionDto
        {
            Id = submission.Id,
            SubmittedByIdentityUserId = submission.SubmittedByIdentityUserId,
            Status = submission.Status,
            SeriesId = submission.SeriesId,
            NewSeriesTitle = submission.NewSeriesTitle,
            NewSeriesOriginalTitle = submission.NewSeriesOriginalTitle,
            NewSeriesType = submission.NewSeriesType,
            NewSeriesStatus = submission.NewSeriesStatus,
            NewSeriesTotalVolumes = submission.NewSeriesTotalVolumes,
            PublisherId = submission.PublisherId,
            NewPublisherName = submission.NewPublisherName,
            NewPublisherUrl = submission.NewPublisherUrl,
            NewPublisherCountryId = submission.NewPublisherCountryId,
            Title = submission.Title,
            Number = submission.Number,
            ISBN = submission.ISBN,
            AgeRestriction = submission.AgeRestriction,
            PurchaseUrl = submission.PurchaseUrl,
            Description = submission.Description,
            IsPreorder = submission.IsPreorder,
            PreorderStart = submission.PreorderStart,
            ReleaseDate = submission.ReleaseDate,
            Type = submission.Type,
            SingleIssue = submission.SingleIssue,
            ApprovedVolumeId = submission.ApprovedVolumeId,
            ReviewComment = submission.ReviewComment,
            SubmittedAt = submission.CreatedAt,
            SeriesTitle = submission.ExistingSeriesTitle ?? submission.NewSeriesTitle ?? string.Empty,
            PublisherName = submission.ExistingPublisherName ?? submission.NewPublisherName ?? string.Empty
        };
    }
}
