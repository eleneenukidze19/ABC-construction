using System.ComponentModel.DataAnnotations;

namespace ABC_construction.DTOs;

/// <summary>Company contact details returned by GET /api/company.</summary>
public class CompanyInformationDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AddressKa { get; set; }
    public string? DescriptionKa { get; set; }
}

/// <summary>
/// Write model for PUT /api/company. Email and phone are deliberately not
/// format-validated: they ship as the literal placeholder "unwritten" until an
/// admin fills in the real values (README section 9).
/// </summary>
public class CompanyInformationWriteDto
{
    [Required(ErrorMessage = "Email is required.")]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [StringLength(60)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(300)]
    public string Address { get; set; } = string.Empty;

    [StringLength(5000)]
    public string? Description { get; set; }

    // Georgian versions, optional: an empty one falls back to English.

    [StringLength(300)]
    public string? AddressKa { get; set; }

    [StringLength(5000)]
    public string? DescriptionKa { get; set; }
}
