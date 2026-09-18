using ABC_construction.DTOs;

namespace ABC_construction.Interfaces;

/// <summary>
/// Business operations for projects (README section 12). Controllers depend on
/// this interface and never touch repositories or the DbContext directly.
/// </summary>
public interface IProjectService
{
    Task<PagedResponse<ProjectSummaryDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? category = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProjectSummaryDto>> GetFeaturedAsync(
        int count = 3,
        CancellationToken cancellationToken = default);

    Task<ProjectDetailDto?> GetByIdAsync(
        int id,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>Categories for the public filter bar, with their Georgian labels.</summary>
    Task<IReadOnlyList<ProjectCategoryDto>> GetCategoryOptionsAsync(CancellationToken cancellationToken = default);

    Task<int> CountAsync(bool includeInactive = false, CancellationToken cancellationToken = default);

    Task<ProjectDetailDto> CreateAsync(ProjectWriteDto dto, CancellationToken cancellationToken = default);

    /// <returns>The updated project, or null when no project with that id exists.</returns>
    Task<ProjectDetailDto?> UpdateAsync(int id, ProjectWriteDto dto, CancellationToken cancellationToken = default);

    /// <returns>False when no project with that id exists.</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<ProjectImageDto?> AddImageAsync(
        int projectId,
        string imageUrl,
        string? caption,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteImageAsync(int imageId, CancellationToken cancellationToken = default);
}
