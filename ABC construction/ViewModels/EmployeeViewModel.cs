namespace ABC_construction.ViewModels;

/// <summary>A staff member as rendered on /employees.</summary>
public class EmployeeViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string? Biography { get; set; }
    public string? ImageUrl { get; set; }

    public string DisplayImageUrl =>
        string.IsNullOrWhiteSpace(ImageUrl) ? "/images/placeholder-person.svg" : ImageUrl;

    /// <summary>Initials shown when no portrait has been uploaded.</summary>
    public string Initials
    {
        get
        {
            var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return "?";
            }

            return parts.Length == 1
                ? Upper(parts[0][0]).ToString()
                : $"{Upper(parts[0][0])}{Upper(parts[^1][0])}";
        }
    }

    /// <summary>
    /// Georgian letters are left as they are: their capital forms (Mtavruli)
    /// are not used for initials and are missing from many fonts.
    /// </summary>
    private static char Upper(char c) =>
        c is >= 'Ⴀ' and <= 'ჿ' or >= 'Ა' and <= 'Ჿ' or >= 'ⴀ' and <= '⴯'
            ? c
            : char.ToUpperInvariant(c);
}

/// <summary>Backing model for the /employees page.</summary>
public class EmployeeListViewModel
{
    public IReadOnlyList<EmployeeViewModel> Employees { get; set; } = Array.Empty<EmployeeViewModel>();
}
