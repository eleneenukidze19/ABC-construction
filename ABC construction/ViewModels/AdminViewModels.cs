using System.ComponentModel.DataAnnotations;

namespace ABC_construction.ViewModels;

/// <summary>Admin sign-in form.</summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "Enter your email address.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter your password.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Keep me signed in")]
    public bool RememberMe { get; set; }

    /// <summary>Where to send the user after a successful sign-in.</summary>
    public string? ReturnUrl { get; set; }
}

/// <summary>
/// Create/edit form for a project. Separate from <c>ProjectWriteDto</c> because
/// the form also carries the uploaded file and the existing gallery, neither of
/// which belong on the API contract.
/// </summary>
public class ProjectFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Enter a project title.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter a category.")]
    [StringLength(100)]
    [Display(Name = "Construction category")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter a short description.")]
    [StringLength(400, MinimumLength = 10, ErrorMessage = "Short description must be between 10 and 400 characters.")]
    [Display(Name = "Short description")]
    public string ShortDescription { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter the full description.")]
    [StringLength(20000, MinimumLength = 10)]
    [Display(Name = "Full description")]
    public string Description { get; set; } = string.Empty;

    [StringLength(60)]
    [Display(Name = "Duration")]
    public string? Duration { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Completion date")]
    public DateTime? CompletionDate { get; set; }

    [StringLength(10000)]
    public string? Timeline { get; set; }

    [StringLength(10000)]
    [Display(Name = "Materials and methods")]
    public string? MaterialsUsed { get; set; }

    [StringLength(10000)]
    [Display(Name = "Challenges")]
    public string? Challenges { get; set; }

    // --- Georgian version. Every field is optional; an empty one shows the
    // English text to visitors reading the site in Georgian. ---

    [StringLength(200)]
    [Display(Name = "Title (Georgian)")]
    public string? TitleKa { get; set; }

    [StringLength(100)]
    [Display(Name = "Construction category (Georgian)")]
    public string? CategoryKa { get; set; }

    [StringLength(400)]
    [Display(Name = "Short description (Georgian)")]
    public string? ShortDescriptionKa { get; set; }

    [StringLength(20000)]
    [Display(Name = "Full description (Georgian)")]
    public string? DescriptionKa { get; set; }

    [StringLength(60)]
    [Display(Name = "Duration (Georgian)")]
    public string? DurationKa { get; set; }

    [StringLength(10000)]
    [Display(Name = "Timeline (Georgian)")]
    public string? TimelineKa { get; set; }

    [StringLength(10000)]
    [Display(Name = "Materials and methods (Georgian)")]
    public string? MaterialsUsedKa { get; set; }

    [StringLength(10000)]
    [Display(Name = "Challenges (Georgian)")]
    public string? ChallengesKa { get; set; }

    [Display(Name = "Visible on the public site")]
    public bool IsActive { get; set; } = true;

    /// <summary>Existing cover image, kept when no replacement is uploaded.</summary>
    public string? ExistingImageUrl { get; set; }

    [Display(Name = "Cover image")]
    public IFormFile? CoverImage { get; set; }

    [Display(Name = "Add gallery image")]
    public IFormFile? GalleryImage { get; set; }

    [StringLength(200)]
    [Display(Name = "Gallery image caption")]
    public string? GalleryCaption { get; set; }

    public List<ProjectImageViewModel> Images { get; set; } = new();

    public bool IsEdit => Id > 0;
}

/// <summary>Create/edit form for an employee.</summary>
public class EmployeeFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Enter the employee's full name.")]
    [StringLength(150, MinimumLength = 2)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter a position.")]
    [StringLength(120, MinimumLength = 2)]
    [Display(Name = "Position")]
    public string Position { get; set; } = string.Empty;

    [StringLength(5000)]
    [Display(Name = "Biography")]
    public string? Biography { get; set; }

    // --- Georgian version; an empty field falls back to English. ---

    [StringLength(150)]
    [Display(Name = "Full name (Georgian)")]
    public string? FullNameKa { get; set; }

    [StringLength(120)]
    [Display(Name = "Position (Georgian)")]
    public string? PositionKa { get; set; }

    [StringLength(5000)]
    [Display(Name = "Biography (Georgian)")]
    public string? BiographyKa { get; set; }

    [Range(0, 9999, ErrorMessage = "Display order must be between 0 and 9999.")]
    [Display(Name = "Display order")]
    public int SortOrder { get; set; }

    [Display(Name = "Visible on the public site")]
    public bool IsActive { get; set; } = true;

    public string? ExistingImageUrl { get; set; }

    [Display(Name = "Portrait")]
    public IFormFile? Portrait { get; set; }

    public bool IsEdit => Id > 0;
}

/// <summary>Company contact details form (/admin/settings).</summary>
public class CompanySettingsViewModel
{
    [Required(ErrorMessage = "Enter an email address.")]
    [StringLength(200)]
    [Display(Name = "Public email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter a phone number.")]
    [StringLength(60)]
    [Display(Name = "Public phone number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter an address.")]
    [StringLength(300)]
    [Display(Name = "Business address")]
    public string Address { get; set; } = string.Empty;

    [StringLength(5000)]
    [Display(Name = "Company description")]
    public string? Description { get; set; }

    // --- Georgian version; an empty field falls back to English. ---

    [StringLength(300)]
    [Display(Name = "Business address (Georgian)")]
    public string? AddressKa { get; set; }

    [StringLength(5000)]
    [Display(Name = "Company description (Georgian)")]
    public string? DescriptionKa { get; set; }
}

/// <summary>Row shape for the admin project and employee listings.</summary>
public class AdminProjectRowViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Duration { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }

    public string DisplayImageUrl =>
        string.IsNullOrWhiteSpace(ImageUrl) ? "/images/placeholder-project.svg" : ImageUrl;
}

public class AdminEmployeeRowViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }

    public string DisplayImageUrl =>
        string.IsNullOrWhiteSpace(ImageUrl) ? "/images/placeholder-person.svg" : ImageUrl;
}
