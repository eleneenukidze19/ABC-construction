namespace ABC_construction.Models;

/// <summary>
/// Role vocabulary for the admin panel. README section 5 requires the
/// architecture to accommodate additional roles later, so roles are declared
/// here once and seeded from <see cref="All"/> rather than hard-coded at
/// each call site — adding a role means adding a constant to this list.
/// </summary>
public static class ApplicationRoles
{
    public const string Admin = "Admin";

    public static readonly IReadOnlyList<string> All = new[] { Admin };

    /// <summary>
    /// The subset of <see cref="All"/> that may sign in to the admin panel.
    /// Every role grants entry today, but keeping the two lists separate means a
    /// later role that only carries permissions on the public site can be added
    /// to <see cref="All"/> without silently gaining panel access.
    /// </summary>
    public static readonly IReadOnlyList<string> PanelRoles = new[] { Admin };

    /// <summary>
    /// True when any of <paramref name="roles"/> permits entry to the panel.
    /// </summary>
    public static bool GrantsPanelAccess(IEnumerable<string> roles) =>
        roles.Any(role => PanelRoles.Contains(role, StringComparer.OrdinalIgnoreCase));
}
