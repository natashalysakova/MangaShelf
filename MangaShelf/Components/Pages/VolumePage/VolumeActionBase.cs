using MangaShelf.BL.Dto;
using MangaShelf.BL.Interfaces;
using MangaShelf.Components.Pages.Admin;
using MangaShelf.Components.Pages.VolumePage.Elements;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using MangaShelf.Localization.Interfaces;
using MangaShelf.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using System.Security.Claims;
using static MangaShelf.Components.Pages.VolumePage.Elements.AddEditReadingDialog;
using static MangaShelf.Components.Pages.VolumePage.Elements.SetOwnershipDialog;

namespace MangaShelf.Components.Pages.VolumePage;

public abstract class VolumeActionBase : ComponentBase, IDisposable
{
    [Inject] protected IDialogService Dialog { get; set; } = default!;
    [Inject] protected IVolumeService VolumeService { get; set; } = default!;
    [Inject] protected IVolumeStateService VolumeState { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected IVolumePageLocalizationService Localizer { get; set; } = default!;
    [Inject] private ILoggerFactory LoggerFactory { get; set; } = default!;

    [CascadingParameter]
    public required Task<AuthenticationState> AuthStateTask { get; set; }

    [Parameter]
    public Guid? VolumeId { get; set; }

    protected virtual Guid? ResolvedVolumeId { get => VolumeId; }

    private ILogger? _logger;
    // Creates a logger categorized under the actual derived type at runtime
    protected ILogger Logger => _logger ??= LoggerFactory.CreateLogger(GetType());

    protected async Task ShowAddEditReadingDialog(ReadingHistoryDto reading)
    {
        var readingfromDb = await VolumeService.GetReading(reading.Id);

        if (readingfromDb == null)
        {
            Logger.LogError("Reading with ReviewId {ReviewId} not found in database.", reading.ReviewId);
            Snackbar.Add(Localizer["ErrorLoadingReading"], Severity.Error);
            return;
        }

        await ShowAddEditReadingDialog(readingfromDb);
    }

    protected async Task ShowAddEditReadingDialog()
    {
        var reading = new Reading()
        {
            Rating = 0,
            StartedAt = DateTime.UtcNow,
            Status = ReadingStatus.PlanToRead,
            VolumeId = ResolvedVolumeId!.Value,
        };

        await ShowAddEditReadingDialog(reading);
    }



    protected async Task ShowAddEditReadingDialog(Reading reading)
    {
        var dialogOptions = new DialogOptions()
        {
            FullWidth = true,
            Position = DialogPosition.Center,
            MaxWidth = MaxWidth.Small,
        };

        var parameters = new DialogParameters
        {
            {
                nameof(AddEditReadingDialog.Reading),
                reading
            }
        };

        var dialog = await Dialog.ShowAsync<AddEditReadingDialog>(string.Empty, parameters, dialogOptions);
        var result = await dialog.Result;

        if (result != null && !result.Canceled)
        {

            try
            {
                var resultData = result.Data as EditReadingDialogResult;
                if (resultData == null)
                {
                    return;
                }

                if (resultData.IsDeleted)
                {
                    var (userStatus, stats) = await VolumeService.RemoveReadingAsync(reading.Id);
                    VolumeState.UpdateBoth(userStatus, stats);
                }
                else
                {
                    var (userStatus, stats) = await VolumeService.AddEditReadingAsync(reading, await GetUserId());
                    VolumeState.UpdateBoth(userStatus, stats);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error adding reading for volume {Id}. Exception:{ex}", VolumeId, ex.Message);
                Snackbar.Add(Localizer["ErrorAddingReading"], Severity.Error);
            }
        }
    }

    protected async Task<UserVolumeStatusDto?> OpenSetOwnershipDialog(VolumeStatus status, bool disableStatus = true)
    {
        var ownership = new Ownership()
        {
            VolumeId = ResolvedVolumeId!.Value,
            Status = status,
            Date = DateTime.UtcNow,
            Type = VolumeType.Physical
        };
        return await OpenSetOwnershipDialog(ownership, disableStatus);
    }

    protected async Task<UserVolumeStatusDto?> OpenSetOwnershipDialog(OwnershipHistoryDto ownership, bool disableStatus = true)
    {

        var ownershipFromDb = await VolumeService.GetOwnership(ownership.Id);

        if (ownershipFromDb == null)
        {
            Logger.LogError("Ownership with Id {Id} not found in database.", ownership.Id);
            Snackbar.Add(Localizer["ErrorLoadingOwnership"], Severity.Error);
            return VolumeState.CurrentUserStatus;
        }

        return await OpenSetOwnershipDialog(ownershipFromDb, disableStatus);
    }

    protected async Task<UserVolumeStatusDto?> OpenSetOwnershipDialog(Ownership ownership, bool disableStatus)
    {
        var dialogOptions = new DialogOptions()
        {
            FullWidth = false,
            Position = DialogPosition.Center,
        };
        var parameters = new DialogParameters
        {
            {   nameof(SetOwnershipDialog.Ownership),
                ownership
            },
            {
                nameof(SetOwnershipDialog.IsDisabledStatus),
                disableStatus
            }
        };

        var dialog = await Dialog.ShowAsync<SetOwnershipDialog>(string.Empty, parameters, dialogOptions);

        var result = await dialog.Result;

        if (result != null && !result.Canceled)
        {
            try
            {
                var resultData = result.Data as SetOwnershipDialogResult;
                if (resultData == null)
                {
                    return VolumeState.CurrentUserStatus;
                }

                if (resultData.IsDeleted)
                {
                    var (userStatus, stats) = await VolumeService.RemoveOwnershipAsync(ownership.Id);
                    VolumeState.UpdateBoth(userStatus, stats);
                }
                else
                {
                    var (userStatus, stats) = await VolumeService.AddEditOwnershipAsync(ownership, await GetUserId());
                    VolumeState.UpdateBoth(userStatus, stats);
                }
            }
            catch (Exception)
            {
                Logger.LogError("Error adding ownership for volume {PublicId}", ResolvedVolumeId!.Value);
            }

        }
        return VolumeState.CurrentUserStatus;

    }

    protected async Task<UserVolumeStatusDto?> ToggleWishList()
    {
        try
        {

            var (userStatus, stats) = await VolumeService.ToggleWishlist(ResolvedVolumeId!.Value, await GetUserId());
            VolumeState.UpdateBoth(userStatus, stats);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error toggling wishlist for Id: {id}", ResolvedVolumeId!.Value);
        }

        return VolumeState.CurrentUserStatus;
    }

    protected async Task<UserVolumeStatusDto?> AddPlanToRead()
    {
        try
        {
            var reading = new Reading()
            {
                VolumeId = ResolvedVolumeId!.Value,
                Status = ReadingStatus.PlanToRead,
                StartedAt = DateTimeOffset.Now.ToLocalTime()
            };

            var (userStatus, stats) = await VolumeService.AddEditReadingAsync(reading, await GetUserId());
            VolumeState.UpdateBoth(userStatus, stats);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error toggling wishlist for Id: {id}", ResolvedVolumeId!.Value);
        }

        return VolumeState.CurrentUserStatus;
    }

    protected async Task ToggleLike()
    {
        try
        {
            var (userStatus, stats) = await VolumeService.ToggleLike(ResolvedVolumeId!.Value, await GetUserId());
            VolumeState.UpdateBoth(userStatus, stats);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error toggling like for Id: {id}", ResolvedVolumeId!.Value);
        }
    }

    protected async Task DeleteFromHistory(Guid ownershipId)
    {
        try
        {
            var (status, stats) = await VolumeService.RemoveOwnershipAsync(ownershipId);
            VolumeState.UpdateBoth(status, stats);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error removing ownership with Id: {Id}", ownershipId);
        }
    }

    protected async Task<string?> GetUserId()
    {
        var authState = await AuthStateTask;
        var userId = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userId;
    }

    protected async Task<bool> IsUserAuthenticated()
    {
        if (AuthStateTask == null)
            return false;

        var authState = await AuthStateTask;
        return authState.User?.Identity?.IsAuthenticated ?? false;
    }

    public virtual void Dispose()
    {
        VolumeState.OnUserStatusChanged -= StateHasChanged;
    }
}
