using MangaShelf.BL.Dto;

namespace MangaShelf.BL.Contracts;

public interface IVolumeSubmissionService
{
    Task SubmitAsync(VolumeSubmissionRequestDto submission, string submittedByIdentityUserId, CancellationToken token = default);
    Task<IReadOnlyList<VolumeSubmissionDto>> GetPendingAsync(CancellationToken token = default);
    Task ApproveAsync(Guid submissionId, string reviewedByIdentityUserId, CancellationToken token = default);
    Task RejectAsync(Guid submissionId, string reviewedByIdentityUserId, string? comment, CancellationToken token = default);
}
