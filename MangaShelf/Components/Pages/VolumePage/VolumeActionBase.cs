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

    override protected async Task OnParametersSetAsync()
    {

    }

    protected async Task ShowAddEditReadingDialog(ReadingHistoryDto? reading)
    {
        var readingfromDb = await VolumeService.GetReading(Guid.Parse(reading.ReviewId));

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
            FullWidth = false,
            Position = DialogPosition.Center,
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
                var (userStatus, stats) = await VolumeService.AddEditReadingAsync(reading, await GetUserId());
                VolumeState.UpdateBoth(userStatus, stats);
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
        var dialogOptions = new DialogOptions()
        {
            FullWidth = false,
            Position = DialogPosition.Center,
        };
        var parameters = new DialogParameters
        {
            {   nameof(SetOwnershipDialog.Ownership),
                new Ownership()
                {
                    Status = status,
                    Date = DateTime.Now,
                    Type = VolumeType.Physical,
                    VolumeId = ResolvedVolumeId!.Value,
                }
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
                var ownership = result.Data as Ownership;

                if (ownership is null)
                {
                    throw new InvalidOperationException("Ownership data is null");
                }

                var (userStatus, stats) = await VolumeService.AddEditOwnershipAsync(ownership, await GetUserId());
                VolumeState.UpdateBoth(userStatus, stats);
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

    public void Dispose()
    {
        VolumeState.Clear();
        VolumeState.OnUserStatusChanged -= StateHasChanged;
    }
}
