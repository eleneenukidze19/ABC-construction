using ABC_construction.Models;

namespace ABC_construction.Data;

/// <summary>
/// Portfolio projects and team members from the client's setup checklist
/// (ABC20-20Checklist.xlsx, tabs 4 and 6). <see cref="DatabaseSeeder"/> loads
/// them once into a new database; after that they are ordinary records,
/// edited or deleted through the admin panel.
/// <para>
/// The Georgian texts are translations of the checklist's English, added with
/// the Georgian version of the site. Databases created before then receive
/// them from the AddGeorgianContent migration instead.
/// </para>
/// </summary>
public static class InitialContent
{
    public const string Category = "Construction & Fit-out";
    public const string CategoryKa = "მშენებლობა და შიდა მოწყობა";

    public const string ShortDescription =
        "Construction and fit-out project delivered as part of ABC Construction's portfolio.";

    public const string ShortDescriptionKa =
        "ABC Construction-ის პორტფოლიოს ფარგლებში განხორციელებული მშენებლობისა და შიდა მოწყობის პროექტი.";

    /// <summary>
    /// In checklist order, with the checklist's full descriptions verbatim. The
    /// checklist gives no duration, completion date, materials or challenges
    /// for any project, so those stay empty and the public pages hide them.
    /// Project names are brand names and stay in Latin script in both languages.
    /// </summary>
    public static readonly (string Title, string Description, string DescriptionKa)[] ProjectRows =
    {
        ("Erisioni Studio",
            "The ABC Construction portfolio identifies Erisioni Studio as one of the company's projects. Specific information about the client's requirements, scope of work, construction process and completed project is not provided in the portfolio.",
            "ABC Construction-ის პორტფოლიოში Erisioni Studio მოხსენიებულია, როგორც კომპანიის ერთ-ერთი პროექტი. პორტფოლიოში არ არის მოცემული კონკრეტული ინფორმაცია დამკვეთის მოთხოვნების, სამუშაოების მოცულობის, მშენებლობის პროცესისა და დასრულებული პროექტის შესახებ."),
        ("Saken",
            "The portfolio identifies Saken as one of ABC Construction's projects. Further project-specific information, including scope, construction process and final result, is not provided.",
            "პორტფოლიოში Saken მოხსენიებულია, როგორც ABC Construction-ის ერთ-ერთი პროექტი. პროექტის შესახებ დამატებითი ინფორმაცია, მათ შორის სამუშაოების მოცულობა, მშენებლობის პროცესი და საბოლოო შედეგი, მოცემული არ არის."),
        ("Kings Garden",
            "Kings Garden is presented as one of the projects in the ABC Construction portfolio. The portfolio does not provide further details regarding the project scope, works performed or construction methods.",
            "Kings Garden წარმოდგენილია, როგორც ABC Construction-ის პორტფოლიოს ერთ-ერთი პროექტი. პორტფოლიოში არ არის მოცემული დამატებითი დეტალები პროექტის მოცულობის, შესრულებული სამუშაოებისა თუ მშენებლობის მეთოდების შესახებ."),
        ("East Point",
            "East Point is identified as one of the projects associated with ABC Construction. No detailed project description, construction duration, materials or challenges are provided in the portfolio.",
            "East Point მოხსენიებულია, როგორც ABC Construction-თან დაკავშირებული ერთ-ერთი პროექტი. პორტფოლიოში არ არის მოცემული პროექტის დეტალური აღწერა, მშენებლობის ხანგრძლივობა, მასალები ან გამოწვევები."),
        ("Tbilisi Gardens",
            "Tbilisi Gardens is presented as one of the company's projects. The available portfolio does not provide sufficient information to describe the project's specific scope or construction methods.",
            "Tbilisi Gardens წარმოდგენილია, როგორც კომპანიის ერთ-ერთი პროექტი. ხელმისაწვდომი პორტფოლიო არ შეიცავს საკმარის ინფორმაციას პროექტის კონკრეტული მოცულობისა თუ მშენებლობის მეთოდების აღსაწერად."),
        ("Domino",
            "Domino is identified as one of ABC Construction's projects. Specific information about the works, materials, duration and project challenges is not included in the portfolio.",
            "Domino მოხსენიებულია, როგორც ABC Construction-ის ერთ-ერთი პროექტი. პორტფოლიოში არ არის მოცემული კონკრეტული ინფორმაცია სამუშაოების, მასალების, ხანგრძლივობისა და პროექტის გამოწვევების შესახებ."),
        ("Radio City",
            "Radio City is presented as one of the company's projects. The portfolio does not provide detailed information about the scope of works, construction process or completed project.",
            "Radio City წარმოდგენილია, როგორც კომპანიის ერთ-ერთი პროექტი. პორტფოლიოში არ არის მოცემული დეტალური ინფორმაცია სამუშაოების მოცულობის, მშენებლობის პროცესისა თუ დასრულებული პროექტის შესახებ."),
        ("Aversi Clinic",
            "Aversi Clinic is identified in the portfolio as one of ABC Construction's projects. No project-specific information regarding duration, materials, methods or construction challenges is provided.",
            "პორტფოლიოში Aversi Clinic მოხსენიებულია, როგორც ABC Construction-ის ერთ-ერთი პროექტი. პროექტის ხანგრძლივობის, მასალების, მეთოდებისა თუ მშენებლობის გამოწვევების შესახებ კონკრეტული ინფორმაცია მოცემული არ არის."),
        ("Metropol Kavtaradze",
            "Metropol Kavtaradze is presented as one of ABC Construction's projects. The portfolio does not contain sufficient technical or project-management information for a more detailed description.",
            "Metropol Kavtaradze წარმოდგენილია, როგორც ABC Construction-ის ერთ-ერთი პროექტი. პორტფოლიო არ შეიცავს საკმარის ტექნიკურ თუ პროექტის მართვასთან დაკავშირებულ ინფორმაციას უფრო დეტალური აღწერისთვის."),
        ("Tabukashvili",
            "Tabukashvili is listed in the portfolio as part of ABC Construction's project experience. Specific details about the scope, materials, duration and challenges are not provided.",
            "Tabukashvili პორტფოლიოში შეტანილია, როგორც ABC Construction-ის საპროექტო გამოცდილების ნაწილი. კონკრეტული დეტალები სამუშაოების მოცულობის, მასალების, ხანგრძლივობისა და გამოწვევების შესახებ მოცემული არ არის."),
        ("CBD Development",
            "CBD Development is identified in the portfolio as one of the company's projects. The portfolio does not provide detailed information about the project's scope, construction methods, duration or challenges.",
            "პორტფოლიოში CBD Development მოხსენიებულია, როგორც კომპანიის ერთ-ერთი პროექტი. პორტფოლიოში არ არის მოცემული დეტალური ინფორმაცია პროექტის მოცულობის, მშენებლობის მეთოდების, ხანგრძლივობისა თუ გამოწვევების შესახებ."),
        ("Seven Hills",
            "Seven Hills is presented in the portfolio as one of ABC Construction's projects. No additional project-specific information is provided.",
            "პორტფოლიოში Seven Hills წარმოდგენილია, როგორც ABC Construction-ის ერთ-ერთი პროექტი. პროექტის შესახებ დამატებითი ინფორმაცია მოცემული არ არის."),
        ("Biography",
            "Biography is listed among the projects in the portfolio. The available material does not provide sufficient information to describe the project's scope, construction process or technical characteristics.",
            "Biography პორტფოლიოში შეტანილია კომპანიის პროექტებს შორის. ხელმისაწვდომი მასალა არ შეიცავს საკმარის ინფორმაციას პროექტის მოცულობის, მშენებლობის პროცესისა თუ ტექნიკური მახასიათებლების აღსაწერად."),
        ("Anagi",
            "Anagi is identified in the portfolio as one of the company's projects. Further details about the works performed, duration, materials and challenges are not provided.",
            "პორტფოლიოში Anagi მოხსენიებულია, როგორც კომპანიის ერთ-ერთი პროექტი. დამატებითი დეტალები შესრულებული სამუშაოების, ხანგრძლივობის, მასალებისა და გამოწვევების შესახებ მოცემული არ არის."),
        ("IMPOST",
            "IMPOST is identified in the portfolio in connection with the Radio City and Aversi Clinic projects. The portfolio does not provide further project-specific information.",
            "პორტფოლიოში IMPOST მოხსენიებულია Radio City-სა და Aversi Clinic-ის პროექტებთან დაკავშირებით. პროექტის შესახებ დამატებითი ინფორმაცია მოცემული არ არის.")
    };

    /// <summary>
    /// Tab 6 of the checklist, plus Grigol Nadirashvili (Founder) added at the
    /// client's request. Names have no Georgian spelling here: the checklist
    /// only gives them in Latin script, and a guessed spelling of someone's
    /// name is worse than none. Admins can add it per person.
    /// </summary>
    private static readonly (string FullName, string Position)[] EmployeeRows =
    {
        ("Grigol Nadirashvili", "Founder"),
        ("Tengiz Savreshiani", "CEO"),
        ("Gaga Bakhturidze", "Project Manager"),
        ("Biktor Akhvlediani", "Project Manager")
    };

    /// <summary>Georgian for each position used in <see cref="EmployeeRows"/>.</summary>
    public static readonly IReadOnlyDictionary<string, string> PositionsKa = new Dictionary<string, string>
    {
        ["Founder"] = "დამფუძნებელი",
        ["CEO"] = "აღმასრულებელი დირექტორი",
        ["Project Manager"] = "პროექტის მენეჯერი"
    };

    public static IReadOnlyList<Project> CreateProjects(DateTime createdUtc) => ProjectRows
        .Select(row => new Project
        {
            Title = row.Title,
            Category = Category,
            CategoryKa = CategoryKa,
            ShortDescription = ShortDescription,
            ShortDescriptionKa = ShortDescriptionKa,
            Description = row.Description,
            DescriptionKa = row.DescriptionKa,
            CreatedDate = createdUtc,
            IsActive = true
        })
        .ToList();

    public static IReadOnlyList<Employee> CreateEmployees(DateTime createdUtc) => EmployeeRows
        .Select((row, index) => new Employee
        {
            FullName = row.FullName,
            Position = row.Position,
            PositionKa = PositionsKa[row.Position],
            SortOrder = index + 1,
            CreatedDate = createdUtc,
            IsActive = true
        })
        .ToList();
}
