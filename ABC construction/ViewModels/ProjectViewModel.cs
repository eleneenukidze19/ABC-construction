namespace ABC_construction.ViewModels;

/// <summary>
/// A project as rendered on a card. README section 15 forbids passing entities
/// into views, so display-ready values are prepared here.
/// </summary>
public class ProjectViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime CompletionDate { get; set; }
    public string? ImageUrl { get; set; }

    /// <summary>Pre-formatted for display, e.g. "March 2024".</summary>
    public string CompletionDateDisplay =>
        CompletionDate.ToString("MMMM yyyy", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>Placeholder keeps card layouts intact when no cover image is set.</summary>
    public string DisplayImageUrl =>
        string.IsNullOrWhiteSpace(ImageUrl) ? "/images/placeholder-project.svg" : ImageUrl;
}

/// <summary>Full project view for /projects/{id}.</summary>
public class ProjectDetailViewModel : ProjectViewModel
{
    public string Description { get; set; } = string.Empty;
    public string? Timeline { get; set; }
    public string? MaterialsUsed { get; set; }
    public string? Challenges { get; set; }
    public List<ProjectImageViewModel> Images { get; set; } = new();

    public bool HasGallery => Images.Count > 0;
}

public class ProjectImageViewModel
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
}

/// <summary>Backing model for the paged /projects listing, including filters.</summary>
public class ProjectListViewModel
{
    public IReadOnlyList<ProjectViewModel> Projects { get; set; } = Array.Empty<ProjectViewModel>();
    public IReadOnlyList<string> Categories { get; set; } = Array.Empty<string>();
    public string? SelectedCategory { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }

    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}
