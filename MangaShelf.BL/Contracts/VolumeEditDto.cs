using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MangaShelf.BL.Dto;
using MangaShelf.DAL.Models;

namespace MangaShelf.BL.Contracts;

public class VolumeEditDto
{
    [ReadOnly(true)]
    public Guid Id { get; set; }
    [ReadOnly(true)]
    public required string PublicId { get; set; }

    [RequiredWhenNotAOneShot]
    public string? Title { get; set; }

    [RequiredWhenNotAOneShot]
    [Range(0, int.MaxValue, ErrorMessage = "Number must be positive")]
    public int? Number { get; set; }

    
    public string? ISBN { get; set; }

    public bool SingleIssue { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "Age restriction must be positive")]
    public int AgeRestriction { get; set; }

    [ReadOnly(true)]
    public string? OriginalCoverUrl { get; set; }
    [ReadOnly(true)]
    public string? CoverImageUrl { get; set; }

    [ReadOnly(true)]
    public string? PurchaseUrl { get; set; }

    public string? Description { get; set; }

    public bool IsPreorder { get; set; }
    public DateTime? PreorderStart { get; set; }

    [Required]
    public DateTime? ReleaseDate { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Type must be selected")]
    [EnumDataType(typeof(VolumeType), ErrorMessage = "Type is invalid")]
    public VolumeType Type { get; set; }

    public Guid SeriesId { get; set; }
    public required string SeriesTitle { get; set; }
    public string? SeriesOriginalTitle { get; set; }
    public required string SeriesPublicId { get; set; }
    public SeriesStatus SeriesStatus { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Total volumes must be positive")]
    public int? SeriesTotalVolumes { get; set; }
    public List<AuthorDto> SeriesAuthors { get; set; } = new();
}

public class RequiredWhenNotAOneShotAttribute : ValidationAttribute
{
    public RequiredWhenNotAOneShotAttribute()
    {
        ErrorMessage = "{0} is required when the volume is not a one-shot.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var volumeEditDto = (VolumeEditDto)validationContext.ObjectInstance;
        var requiresValue = volumeEditDto.SeriesStatus != SeriesStatus.OneShot;
        var isMissing = value == null || (value is string text && string.IsNullOrWhiteSpace(text));

        if (requiresValue && isMissing)
        {
            var memberName = validationContext.MemberName ?? string.Empty;
            var error = string.Format(ErrorMessageString, validationContext.DisplayName);
            return new ValidationResult(error, new[] { memberName });
        }

        return ValidationResult.Success;
    }
}