namespace ABC_construction.ViewModels;

/// <summary>Summary counts shown on /admin/dashboard.</summary>
public class AdminDashboardViewModel
{
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }

    public int HiddenProjects => TotalProjects - ActiveProjects;

    /// <summary>Prompts the admin to replace the seeded "unwritten" placeholders.</summary>
    public bool ContactDetailsIncomplete { get; set; }

    public IReadOnlyList<ProjectViewModel> RecentProjects { get; set; } = Array.Empty<ProjectViewModel>();
}
