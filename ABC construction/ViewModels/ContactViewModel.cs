using ABC_construction.Models;

namespace ABC_construction.ViewModels;

/// <summary>Backing model for /contact.</summary>
public class ContactViewModel
{
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// True while a value is still the seeded "unwritten" placeholder, so the
    /// view can render it as pending rather than as a broken mailto/tel link.
    /// </summary>
    public bool IsEmailSet => IsSet(Email);
    public bool IsPhoneSet => IsSet(PhoneNumber);
    public bool IsAddressSet => IsSet(Address);

    private static bool IsSet(string value) =>
        !string.IsNullOrWhiteSpace(value)
        && !string.Equals(value, CompanyInformation.Unwritten, StringComparison.OrdinalIgnoreCase);
}
