using System.ComponentModel.DataAnnotations;

namespace ABC_construction.DTOs;

/// <summary>Employee payload returned by GET /api/employees.</summary>
public class EmployeeDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string? Biography { get; set; }
    public string? ImageUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>Write model for POST/PUT /api/employees.</summary>
public class EmployeeWriteDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(150, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Position is required.")]
    [StringLength(120, MinimumLength = 2)]
    public string Position { get; set; } = string.Empty;

    [StringLength(5000)]
    public string? Biography { get; set; }

    [StringLength(400)]
    public string? ImageUrl { get; set; }

    [Range(0, 9999)]
    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
