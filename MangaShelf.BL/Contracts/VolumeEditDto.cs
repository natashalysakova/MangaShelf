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

    [Required]
    public required string Title { get; set; }

    [Required]
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