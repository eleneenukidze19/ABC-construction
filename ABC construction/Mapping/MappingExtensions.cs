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
        IsActive = project.IsActive
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
    // controllers, so an entity never reaches the MVC layer at all.

    public static ProjectViewModel ToViewModel(this ProjectSummaryDto dto) => new()
    {
        Id = dto.Id,
        Title = dto.Title,
        ShortDescription = dto.ShortDescription,
        Duration = dto.Duration,
        Category = dto.Category,
        CompletionDate = dto.CompletionDate,
        ImageUrl = dto.ImageUrl
    };

    public static ProjectDetailViewModel ToDetailViewModel(this ProjectDetailDto dto) => new()
    {
        Id = dto.Id,
        Title = dto.Title,
        ShortDescription = dto.ShortDescription,
        Description = dto.Description,
        Duration = dto.Duration,
        Category = dto.Category,
        CompletionDate = dto.CompletionDate,
        ImageUrl = dto.ImageUrl,
        Timeline = dto.Timeline,
        MaterialsUsed = dto.MaterialsUsed,
        Challenges = dto.Challenges,
        Images = dto.Images
            .Select(i => new ProjectImageViewModel
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                Caption = i.Caption
            })
            .ToList()
    };

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
        project.IsActive = dto.IsActive;
    }

    // --- Employee ---

    public static EmployeeDto ToDto(this Employee employee) => new()
    {
        Id = employee.Id,
        FullName = employee.FullName,
        Position = employee.Position,
        Biography = employee.Biography,
        ImageUrl = employee.ImageUrl,
        SortOrder = employee.SortOrder,
        IsActive = employee.IsActive
    };

    public static EmployeeViewModel ToViewModel(this EmployeeDto dto) => new()
    {
        Id = dto.Id,
        FullName = dto.FullName,
        Position = dto.Position,
        Biography = dto.Biography,
        ImageUrl = dto.ImageUrl
    };

    public static void ApplyTo(this EmployeeWriteDto dto, Employee employee)
    {
        employee.FullName = dto.FullName.Trim();
        employee.Position = dto.Position.Trim();
        employee.Biography = Normalise(dto.Biography);
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
        Description = info.Description
    };

    public static ContactViewModel ToViewModel(this CompanyInformationDto dto) => new()
    {
        Email = dto.Email,
        PhoneNumber = dto.PhoneNumber,
        Address = dto.Address,
        Description = dto.Description
    };

    public static void ApplyTo(this CompanyInformationWriteDto dto, CompanyInformation info)
    {
        info.Email = dto.Email.Trim();
        info.PhoneNumber = dto.PhoneNumber.Trim();
        info.Address = dto.Address.Trim();
        info.Description = Normalise(dto.Description);
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
