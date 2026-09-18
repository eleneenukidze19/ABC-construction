using ABC_construction.Models;

namespace ABC_construction.Interfaces;

/// <summary>
/// Data access for <see cref="Project"/>. Queries, CRUD, filtering and
/// pagination only — no business rules (README section 11).
/// </summary>
public interface IProjectRepository
{
    /// <param name="includeInactive">
    /// Public pages pass false; the admin panel passes true so hidden projects
    /// remain manageable.
    /// </param>
    Task<PagedResult<Project>> GetPagedAsync(
        int page,
        int pageSize,
        string? category = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Project>> GetFeaturedAsync(
        int count,
        CancellationToken cancellationToken = default);

    /// <param name="includeImages">Eager-loads the gallery for the detail page.</param>
    Task<Project?> GetByIdAsync(
        int id,
        bool includeImages = false,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Active categories paired with a Georgian label, taken from whichever
    /// project in the category has one filled in (null if none do).
    /// </summary>
    Task<IReadOnlyList<(string Name, string? NameKa)>> GetCategoryLabelsAsync(CancellationToken cancellationToken = default);

    Task<int> CountAsync(bool includeInactive = false, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(Project project, CancellationToken cancellationToken = default);

    Task UpdateAsync(Project project, CancellationToken cancellationToken = default);

    Task DeleteAsync(Project project, CancellationToken cancellationToken = default);

    Task AddImageAsync(ProjectImage image, CancellationToken cancellationToken = default);

    Task<ProjectImage?> GetImageAsync(int imageId, CancellationToken cancellationToken = default);

    Task DeleteImageAsync(ProjectImage image, CancellationToken cancellationToken = default);
}
