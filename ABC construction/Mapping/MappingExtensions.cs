using ABC_construction.DTOs;
using ABC_construction.Models;
using ABC_construction.ViewModels;

namespace ABC_construction.Mapping;

/// <summary>
/// Entity to DTO/ViewModel translation. Centralised so the boundary rules from
/// README sections 12 and 15 (entities never reach API responses or views) are
/// enforced in one place instead of being re-implemented per service.
/// </summary>
public static class MappingExtensions
{
    // --- Project ---

    public static ProjectSummaryDto ToSummaryDto(this Project project) => new()
    {
        Id = project.Id,
        Title = project.Title,
        ShortDescription = project.ShortDescription,
        Duration = project.Duration,
        CompletionDate = project.CompletionDate,
        Category = project.Category,
        ImageUrl = project.ImageUrl,
        IsActive = project.IsActive,
        TitleKa = project.TitleKa,
        ShortDescriptionKa = project.ShortDescriptionKa,
        DurationKa = project.DurationKa,
        CategoryKa = project.CategoryKa
    };

    public static ProjectDetailDto ToDetailDto(this Project project) => new()
    {
        Id = project.Id,
        Title = project.Title,
        ShortDescription = project.ShortDescription,
        Description = project.Description,
        Duration = project.Duration,
        CompletionDate = project.CompletionDate,
        Category = project.Category,
        ImageUrl = project.ImageUrl,
        Timeline = project.Timeline,
        MaterialsUsed = project.MaterialsUsed,
        Challenges = project.Challenges,
        CreatedDate = project.CreatedDate,
        UpdatedDate = project.UpdatedDate,
        IsActive = project.IsActive,
        TitleKa = project.TitleKa,
        ShortDescriptionKa = project.ShortDescriptionKa,
        DurationKa = project.DurationKa,
        CategoryKa = project.CategoryKa,
        DescriptionKa = project.DescriptionKa,
        TimelineKa = project.TimelineKa,
        MaterialsUsedKa = project.MaterialsUsedKa,
        ChallengesKa = project.ChallengesKa,
        Images = project.Images
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.Id)
            .Select(i => new ProjectImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                Caption = i.Caption,
                SortOrder = i.SortOrder
            })
            .ToList()
    };

    // View models are built from DTOs, not entities: services hand DTOs to
    // controllers, so an entity never reaches the MVC layer at all. They are
    // also where the request's language is applied (see ContentLanguage).

    public static ProjectViewModel ToViewModel(this ProjectSummaryDto dto) => new()
    {
        Id = dto.Id,
        Title = ContentLanguage.Pick(dto.Title, dto.TitleKa),
        ShortDescription = ContentLanguage.Pick(dto.ShortDescription, dto.ShortDescriptionKa),
        Duration = ContentLanguage.PickOptional(dto.Duration, dto.DurationKa),
        Category = ContentLanguage.Pick(dto.Category, dto.CategoryKa),
        CompletionDate = dto.CompletionDate,
        ImageUrl = dto.ImageUrl
    };

    public static ProjectDetailViewModel ToDetailViewModel(this ProjectDetailDto dto) => new()
    {
        Id = dto.Id,
        Title = ContentLanguage.Pick(dto.Title, dto.TitleKa),
        ShortDescription = ContentLanguage.Pick(dto.ShortDescription, dto.ShortDescriptionKa),
        Description = ContentLanguage.Pick(dto.Description, dto.DescriptionKa),
        Duration = ContentLanguage.PickOptional(dto.Duration, dto.DurationKa),
        Category = ContentLanguage.Pick(dto.Category, dto.CategoryKa),
        CompletionDate = dto.CompletionDate,
        ImageUrl = dto.ImageUrl,
        Timeline = ContentLanguage.PickOptional(dto.Timeline, dto.TimelineKa),
        MaterialsUsed = ContentLanguage.PickOptional(dto.MaterialsUsed, dto.MaterialsUsedKa),
        Challenges = ContentLanguage.PickOptional(dto.Challenges, dto.ChallengesKa),
        Images = dto.Images
            .Select(i => new ProjectImageViewModel
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                Caption = i.Caption
            })
            .ToList()
    };

    public static ProjectCategoryOption ToOption(this ProjectCategoryDto dto) =>
        new(dto.Name, ContentLanguage.Pick(dto.Name, dto.NameKa));

    /// <summary>Applies a write DTO onto a new or existing entity.</summary>
    public static void ApplyTo(this ProjectWriteDto dto, Project project)
    {
        project.Title = dto.Title.Trim();
        project.Description = dto.Description.Trim();
        project.ShortDescription = dto.ShortDescription.Trim();
        project.Duration = Normalise(dto.Duration);
        // Postgres 'timestamp with time zone' requires UTC through Npgsql.
        project.CompletionDate = dto.CompletionDate is { } date
            ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
            : null;
        project.Category = dto.Category.Trim();
        project.ImageUrl = Normalise(dto.ImageUrl);
        project.Timeline = Normalise(dto.Timeline);
        project.MaterialsUsed = Normalise(dto.MaterialsUsed);
        project.Challenges = Normalise(dto.Challenges);
        project.TitleKa = Normalise(dto.TitleKa);
        project.DescriptionKa = Normalise(dto.DescriptionKa);
        project.ShortDescriptionKa = Normalise(dto.ShortDescriptionKa);
        project.DurationKa = Normalise(dto.DurationKa);
        project.CategoryKa = Normalise(dto.CategoryKa);
        project.TimelineKa = Normalise(dto.TimelineKa);
        project.MaterialsUsedKa = Normalise(dto.MaterialsUsedKa);
        project.ChallengesKa = Normalise(dto.ChallengesKa);
        project.IsActive = dto.IsActive;
    }

    // --- Employee ---

    public static EmployeeDto ToDto(this Employee employee) => new()
    {
        Id = employee.Id,
        FullName = employee.FullName,
        Position = employee.Position,
        Biography = employee.Biography,
        FullNameKa = employee.FullNameKa,
        PositionKa = employee.PositionKa,
        BiographyKa = employee.BiographyKa,
        ImageUrl = employee.ImageUrl,
        SortOrder = employee.SortOrder,
        IsActive = employee.IsActive
    };

    public static EmployeeViewModel ToViewModel(this EmployeeDto dto) => new()
    {
        Id = dto.Id,
        FullName = ContentLanguage.Pick(dto.FullName, dto.FullNameKa),
        Position = ContentLanguage.Pick(dto.Position, dto.PositionKa),
        Biography = ContentLanguage.PickOptional(dto.Biography, dto.BiographyKa),
        ImageUrl = dto.ImageUrl
    };

    public static void ApplyTo(this EmployeeWriteDto dto, Employee employee)
    {
        employee.FullName = dto.FullName.Trim();
        employee.Position = dto.Position.Trim();
        employee.Biography = Normalise(dto.Biography);
        employee.FullNameKa = Normalise(dto.FullNameKa);
        employee.PositionKa = Normalise(dto.PositionKa);
        employee.BiographyKa = Normalise(dto.BiographyKa);
        employee.ImageUrl = Normalise(dto.ImageUrl);
        employee.SortOrder = dto.SortOrder;
        employee.IsActive = dto.IsActive;
    }

    // --- Company information ---

    public static CompanyInformationDto ToDto(this CompanyInformation info) => new()
    {
        Id = info.Id,
        Email = info.Email,
        PhoneNumber = info.PhoneNumber,
        Address = info.Address,
        Description = info.Description,
        AddressKa = info.AddressKa,
        DescriptionKa = info.DescriptionKa
    };

    public static ContactViewModel ToViewModel(this CompanyInformationDto dto) => new()
    {
        Email = dto.Email,
        PhoneNumber = dto.PhoneNumber,
        Address = ContentLanguage.Pick(dto.Address, dto.AddressKa),
        Description = ContentLanguage.PickOptional(dto.Description, dto.DescriptionKa)
    };

    public static void ApplyTo(this CompanyInformationWriteDto dto, CompanyInformation info)
    {
        info.Email = dto.Email.Trim();
        info.PhoneNumber = dto.PhoneNumber.Trim();
        info.Address = dto.Address.Trim();
        info.Description = Normalise(dto.Description);
        info.AddressKa = Normalise(dto.AddressKa);
        info.DescriptionKa = Normalise(dto.DescriptionKa);
    }

    // --- Paging ---

    public static PagedResponse<T> ToResponse<T>(this PagedResult<T> result) => new()
    {
        Items = result.Items,
        TotalCount = result.TotalCount,
        Page = result.Page,
        PageSize = result.PageSize,
        TotalPages = result.TotalPages,
        HasPrevious = result.HasPrevious,
        HasNext = result.HasNext
    };

    /// <summary>Collapses whitespace-only optional input to null for consistent storage.</summary>
    private static string? Normalise(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
