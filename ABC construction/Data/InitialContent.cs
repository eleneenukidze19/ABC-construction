using ABC_construction.Models;

namespace ABC_construction.Data;

/// <summary>
/// Portfolio projects and team members from the client's setup checklist
/// (ABC20-20Checklist.xlsx, tabs 4 and 6). <see cref="DatabaseSeeder"/> loads
/// them once into a new database; after that they are ordinary records,
/// edited or deleted through the admin panel.
/// </summary>
public static class InitialContent
{
    private const string Category = "Construction & Fit-out";

    private const string ShortDescription =
        "Construction and fit-out project delivered as part of ABC Construction's portfolio.";

    /// <summary>
    /// In checklist order, with the checklist's full descriptions verbatim. The
    /// checklist gives no duration, completion date, materials or challenges
    /// for any project, so those stay empty and the public pages hide them.
    /// </summary>
    private static readonly (string Title, string Description)[] ProjectRows =
    {
        ("Erisioni Studio", "The ABC Construction portfolio identifies Erisioni Studio as one of the company's projects. Specific information about the client's requirements, scope of work, construction process and completed project is not provided in the portfolio."),
        ("Saken", "The portfolio identifies Saken as one of ABC Construction's projects. Further project-specific information, including scope, construction process and final result, is not provided."),
        ("Kings Garden", "Kings Garden is presented as one of the projects in the ABC Construction portfolio. The portfolio does not provide further details regarding the project scope, works performed or construction methods."),
        ("East Point", "East Point is identified as one of the projects associated with ABC Construction. No detailed project description, construction duration, materials or challenges are provided in the portfolio."),
        ("Tbilisi Gardens", "Tbilisi Gardens is presented as one of the company's projects. The available portfolio does not provide sufficient information to describe the project's specific scope or construction methods."),
        ("Domino", "Domino is identified as one of ABC Construction's projects. Specific information about the works, materials, duration and project challenges is not included in the portfolio."),
        ("Radio City", "Radio City is presented as one of the company's projects. The portfolio does not provide detailed information about the scope of works, construction process or completed project."),
        ("Aversi Clinic", "Aversi Clinic is identified in the portfolio as one of ABC Construction's projects. No project-specific information regarding duration, materials, methods or construction challenges is provided."),
        ("Metropol Kavtaradze", "Metropol Kavtaradze is presented as one of ABC Construction's projects. The portfolio does not contain sufficient technical or project-management information for a more detailed description."),
        ("Tabukashvili", "Tabukashvili is listed in the portfolio as part of ABC Construction's project experience. Specific details about the scope, materials, duration and challenges are not provided."),
        ("CBD Development", "CBD Development is identified in the portfolio as one of the company's projects. The portfolio does not provide detailed information about the project's scope, construction methods, duration or challenges."),
        ("Seven Hills", "Seven Hills is presented in the portfolio as one of ABC Construction's projects. No additional project-specific information is provided."),
        ("Biography", "Biography is listed among the projects in the portfolio. The available material does not provide sufficient information to describe the project's scope, construction process or technical characteristics."),
        ("Anagi", "Anagi is identified in the portfolio as one of the company's projects. Further details about the works performed, duration, materials and challenges are not provided."),
        ("IMPOST", "IMPOST is identified in the portfolio in connection with the Radio City and Aversi Clinic projects. The portfolio does not provide further project-specific information.")
    };

    /// <summary>
    /// Tab 6 of the checklist, plus Grigol Nadirashvili (Founder) added at the
    /// client's request.
    /// </summary>
    private static readonly (string FullName, string Position)[] EmployeeRows =
    {
        ("Grigol Nadirashvili", "Founder"),
        ("Tengiz Savreshiani", "CEO"),
        ("Gaga Bakhturidze", "Project Manager"),
        ("Biktor Akhvlediani", "Project Manager")
    };

    public static IReadOnlyList<Project> CreateProjects(DateTime createdUtc) => ProjectRows
        .Select(row => new Project
        {
            Title = row.Title,
            Category = Category,
            ShortDescription = ShortDescription,
            Description = row.Description,
            CreatedDate = createdUtc,
            IsActive = true
        })
        .ToList();

    public static IReadOnlyList<Employee> CreateEmployees(DateTime createdUtc) => EmployeeRows
        .Select((row, index) => new Employee
        {
            FullName = row.FullName,
            Position = row.Position,
            SortOrder = index + 1,
            CreatedDate = createdUtc,
            IsActive = true
        })
        .ToList();
}
