using System.ComponentModel.DataAnnotations;

namespace ABC_construction.Models;

/// <summary>
/// Singleton row holding the contact details shown on /contact.
/// Editable from the admin panel (/admin/settings).
/// </summary>
public class CompanyInformation
{
    /// <summary>Fixed id of the single row this table is expected to hold.</summary>
    public const int SingletonId = 1;

    public int Id { get; set; }

    [MaxLength(200)]
    public string Email { get; set; } = Unwritten;

    [MaxLength(60)]
    public string PhoneNumber { get; set; } = Unwritten;

    [MaxLength(300)]
    public string Address { get; set; } = Unwritten;

    public string? Description { get; set; }

    // --- Georgian versions; an empty one falls back to English. Email and
    // phone need no translation. ---

    [MaxLength(300)]
    public string? AddressKa { get; set; }

    public string? DescriptionKa { get; set; }

    /// <summary>
    /// Placeholder required by README section 9 until the real details are
    /// entered through the admin panel.
    /// </summary>
    public const string Unwritten = "unwritten";
}
