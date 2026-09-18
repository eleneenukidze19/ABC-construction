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
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public List<ProjectImageDto> Images { get; set; } = new();
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

    public bool IsActive { get; set; } = true;
}
