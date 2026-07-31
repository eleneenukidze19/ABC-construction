using System.ComponentModel.DataAnnotations;

namespace ABC_construction.Models;

/// <summary>
/// A gallery image belonging to a <see cref="Project"/>. One Project -> many images.
/// </summary>
public class ProjectImage
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    [Required]
    [MaxLength(400)]
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>Alt text for accessibility; falls back to the project title when empty.</summary>
    [MaxLength(200)]
    public string? Caption { get; set; }

    /// <summary>Controls gallery ordering in the admin panel.</summary>
    public int SortOrder { get; set; }

    public Project? Project { get; set; }
}
