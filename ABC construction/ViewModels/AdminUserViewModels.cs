using System.ComponentModel.DataAnnotations;

namespace ABC_construction.ViewModels;

/// <summary>Row in the /admin/users listing.</summary>
public class AdminUserRowViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
    public DateTime CreatedDate { get; set; }
    public bool IsLockedOut { get; set; }

    /// <summary>Marks the signed-in account so the UI can label it and hide self-destructive actions.</summary>
    public bool IsCurrentUser { get; set; }

    public string DisplayNameOrEmail =>
        string.IsNullOrWhiteSpace(DisplayName) ? Email : DisplayName;

    public bool HasNoRoles => Roles.Count == 0;
}

/// <summary>Backing model for the /admin/users list page.</summary>
public class AdminUserListViewModel
{
    public IReadOnlyList<AdminUserRowViewModel> Users { get; set; } = Array.Empty<AdminUserRowViewModel>();

    /// <summary>
    /// Drives the "last administrator" warning, and lets the view explain why
    /// certain delete buttons are unavailable.
    /// </summary>
    public int AdministratorCount { get; set; }
}

/// <summary>Create-account form.</summary>
public class CreateUserViewModel
{
    [Required(ErrorMessage = "Enter an email address.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(200)]
    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    [StringLength(150)]
    [Display(Name = "Display name")]
    public string? DisplayName { get; set; }

    // Mirrors the Identity policy configured in Program.cs. Kept in sync by
    // hand: Identity validates server-side regardless, this just fails fast
    // with a clearer message.
    [Required(ErrorMessage = "Enter a password.")]
    [StringLength(128, MinimumLength = 10, ErrorMessage = "Password must be at least 10 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm the password.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "The two passwords do not match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Roles")]
    public List<string> SelectedRoles { get; set; } = new();
}

/// <summary>Edit-account form.</summary>
public class EditUserViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter an email address.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(200)]
    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    [StringLength(150)]
    [Display(Name = "Display name")]
    public string? DisplayName { get; set; }

    [Display(Name = "Roles")]
    public List<string> SelectedRoles { get; set; } = new();

    /// <summary>Optional: blank leaves the existing password untouched.</summary>
    [StringLength(128, MinimumLength = 10, ErrorMessage = "Password must be at least 10 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "The two passwords do not match.")]
    [Display(Name = "Confirm new password")]
    public string? ConfirmNewPassword { get; set; }

    public bool IsCurrentUser { get; set; }
    public bool IsLockedOut { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }

    /// <summary>
    /// True when this account is the only administrator, so the view can
    /// disable the Admin checkbox and explain why.
    /// </summary>
    public bool IsOnlyAdministrator { get; set; }
}
