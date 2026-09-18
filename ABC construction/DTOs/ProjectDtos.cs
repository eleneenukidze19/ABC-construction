using System.ComponentModel.DataAnnotations;

namespace ABC_construction.DTOs;

/// <summary>Project summary returned by GET /api/projects and used on cards.</summary>
public class ProjectSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? Duration { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }

    // Georgian versions; null when not translated.
    public string? TitleKa { get; set; }
    public string? ShortDescriptionKa { get; set; }
    public string? DurationKa { get; set; }
    public string? CategoryKa { get; set; }

    /// <summary>
    /// Always true on public responses, which only ever contain active
    /// projects. Carried on the summary so the admin listing can show the
    /// live/hidden badge without re-querying each row.
    /// </summary>
    public bool IsActive { get; set; }
}

/// <summary>Full project payload returned by GET /api/projects/{id}.</summary>
public class ProjectDetailDto : ProjectSummaryDto
{
    public string Description { get; set; } = string.Empty;
    public string? Timeline { get; set; }
    public string? MaterialsUsed { get; set; }
    public string? Challenges { get; set; }
    public string? DescriptionKa { get; set; }
    public string? TimelineKa { get; set; }
    public string? MaterialsUsedKa { get; set; }
    public string? ChallengesKa { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public List<ProjectImageDto> Images { get; set; } = new();
}

/// <summary>A category on the public filter bar, with its Georgian label if one was entered.</summary>
public class ProjectCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameKa { get; set; }
}

public class ProjectImageDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>
/// Write model for POST/PUT /api/projects. Kept separate from the entity so
/// callers can never set Id, CreatedDate or the image collection directly.
/// </summary>
public class ProjectWriteDto
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(20000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Short description is required.")]
    [StringLength(400, MinimumLength = 10)]
    public string ShortDescription { get; set; } = string.Empty;

    [StringLength(60)]
    public string? Duration { get; set; }

    public DateTime? CompletionDate { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [StringLength(400)]
    public string? ImageUrl { get; set; }

    [StringLength(10000)]
    public string? Timeline { get; set; }

    [StringLength(10000)]
    public string? MaterialsUsed { get; set; }

    [StringLength(10000)]
    public string? Challenges { get; set; }

    // Georgian versions, all optional: an empty one falls back to English.

    [StringLength(200)]
    public string? TitleKa { get; set; }

    [StringLength(20000)]
    public string? DescriptionKa { get; set; }

    [StringLength(400)]
    public string? ShortDescriptionKa { get; set; }

    [StringLength(60)]
    public string? DurationKa { get; set; }

    [StringLength(100)]
    public string? CategoryKa { get; set; }

    [StringLength(10000)]
    public string? TimelineKa { get; set; }

    [StringLength(10000)]
    public string? MaterialsUsedKa { get; set; }

    [StringLength(10000)]
    public string? ChallengesKa { get; set; }

    public bool IsActive { get; set; } = true;
}
