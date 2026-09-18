using System.ComponentModel.DataAnnotations;

namespace ABC_construction.Models;

/// <summary>
/// A member of staff shown on /employees.
/// </summary>
public class Employee
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Job title, e.g. "Project Manager", "Architect", "Engineer".</summary>
    [Required]
    [MaxLength(120)]
    public string Position { get; set; } = string.Empty;

    public string? Biography { get; set; }

    // --- Georgian versions; an empty one falls back to English. ---

    [MaxLength(150)]
    public string? FullNameKa { get; set; }

    [MaxLength(120)]
    public string? PositionKa { get; set; }

    public string? BiographyKa { get; set; }

    [MaxLength(400)]
    public string? ImageUrl { get; set; }

    /// <summary>Controls display order on the employees page.</summary>
    public int SortOrder { get; set; }

    /// <summary>Hides former staff without deleting their record.</summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; }
}
