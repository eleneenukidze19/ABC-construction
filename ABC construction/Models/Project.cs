using System.ComponentModel.DataAnnotations;

namespace ABC_construction.Models;

/// <summary>
/// A completed (or in-progress) construction project shown on /projects.
/// </summary>
public class Project
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Full description rendered on the project detail page.</summary>
    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>Teaser copy used on project cards in the listing page.</summary>
    [Required]
    [MaxLength(400)]
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable duration as shown to visitors, e.g. "8 months". Optional:
    /// older portfolio projects often have no reliable figure, and the site
    /// hides the field rather than showing a placeholder.
    /// </summary>
    [MaxLength(60)]
    public string? Duration { get; set; }

    /// <summary>Optional for the same reason as <see cref="Duration"/>.</summary>
    public DateTime? CompletionDate { get; set; }

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    /// <summary>Cover image. Gallery images live in <see cref="Images"/>.</summary>
    [MaxLength(400)]
    public string? ImageUrl { get; set; }

    // --- Fields beyond README section 9, required by the /projects/{id} detail
    // page described in README section 4 (timeline, materials, challenges). ---

    /// <summary>Narrative timeline of the build, e.g. milestone-per-line text.</summary>
    public string? Timeline { get; set; }

    /// <summary>Technologies and materials used on the project.</summary>
    public string? MaterialsUsed { get; set; }

    /// <summary>Notable engineering challenges and how they were solved.</summary>
    public string? Challenges { get; set; }

    // --- Georgian versions of the text fields above. All optional: an empty
    // one falls back to the English text on the Georgian site. ---

    [MaxLength(200)]
    public string? TitleKa { get; set; }

    public string? DescriptionKa { get; set; }

    [MaxLength(400)]
    public string? ShortDescriptionKa { get; set; }

    [MaxLength(60)]
    public string? DurationKa { get; set; }

    /// <summary>Display label only; filtering always uses <see cref="Category"/>.</summary>
    [MaxLength(100)]
    public string? CategoryKa { get; set; }

    public string? TimelineKa { get; set; }

    public string? MaterialsUsedKa { get; set; }

    public string? ChallengesKa { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    /// <summary>Soft-delete / visibility flag. Public pages only show active projects.</summary>
    public bool IsActive { get; set; } = true;

    public ICollection<ProjectImage> Images { get; set; } = new List<ProjectImage>();
}
