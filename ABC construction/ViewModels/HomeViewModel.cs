namespace ABC_construction.ViewModels;

/// <summary>
/// Backing model for the landing page (README section 4): hero, featured
/// projects, statistics, services overview and the contact block.
/// </summary>
public class HomeViewModel
{
    public IReadOnlyList<ProjectViewModel> FeaturedProjects { get; set; } = Array.Empty<ProjectViewModel>();
    public ContactViewModel Contact { get; set; } = new();

    /// <summary>Company statistics strip.</summary>
    public int CompletedProjectsCount { get; set; }
    public int TeamSize { get; set; }
    public int YearsInBusiness { get; set; }
}
