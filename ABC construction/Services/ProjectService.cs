using ABC_construction.Configuration;
using ABC_construction.DTOs;
using ABC_construction.Interfaces;
using ABC_construction.Mapping;
using ABC_construction.Models;
using Microsoft.Extensions.Options;

namespace ABC_construction.Services;

/// <inheritdoc cref="IProjectService"/>
public class ProjectService : IProjectService
{
    /// <summary>Upper bound on page size so a crafted query cannot pull the whole table.</summary>
    private const int MaxPageSize = 60;

    private readonly IProjectRepository _repository;
    private readonly ICacheService _cache;
    private readonly CachingOptions _cachingOptions;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(
        IProjectRepository repository,
        ICacheService cache,
        IOptions<CachingOptions> cachingOptions,
        ILogger<ProjectService> logger)
    {
        _repository = repository;
        _cache = cache;
        _cachingOptions = cachingOptions.Value;
        _logger = logger;
    }

    public async Task<PagedResponse<ProjectSummaryDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? category = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();

        // Admin views must never read from cache: they include hidden projects
        // and need to reflect a write that happened moments ago.
        if (includeInactive)
        {
            var adminResult = await _repository.GetPagedAsync(
                page, pageSize, category, includeInactive: true, cancellationToken);
            return adminResult.Map(p => p.ToSummaryDto()).ToResponse();
        }

        return await _cache.GetOrCreateAsync(
            CacheRegions.Projects,
            $"paged:{page}:{pageSize}:{category ?? "all"}",
            async ct =>
            {
                var result = await _repository.GetPagedAsync(page, pageSize, category, false, ct);
                return result.Map(p => p.ToSummaryDto()).ToResponse();
            },
            _cachingOptions.ProjectsTtl,
            cancellationToken);
    }

    public async Task<IReadOnlyList<ProjectSummaryDto>> GetFeaturedAsync(
        int count = 3,
        CancellationToken cancellationToken = default)
    {
        count = Math.Clamp(count, 1, 12);

        return await _cache.GetOrCreateAsync(
            CacheRegions.Projects,
            $"featured:{count}",
            async ct =>
            {
                var projects = await _repository.GetFeaturedAsync(count, ct);
                return (IReadOnlyList<ProjectSummaryDto>)projects
                    .Select(p => p.ToSummaryDto())
                    .ToList();
            },
            _cachingOptions.ProjectsTtl,
            cancellationToken);
    }

    public async Task<ProjectDetailDto?> GetByIdAsync(
        int id,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        if (includeInactive)
        {
            var adminProject = await _repository.GetByIdAsync(id, true, true, cancellationToken);
            return adminProject?.ToDetailDto();
        }

        return await _cache.GetOrCreateAsync<ProjectDetailDto?>(
            CacheRegions.Projects,
            $"detail:{id}",
            async ct =>
            {
                var project = await _repository.GetByIdAsync(id, includeImages: true,
                    includeInactive: false, cancellationToken: ct);
                return project?.ToDetailDto();
            },
            _cachingOptions.ProjectsTtl,
            cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheRegions.Projects,
            "categories",
            async ct => await _repository.GetCategoriesAsync(ct),
            _cachingOptions.ProjectsTtl,
            cancellationToken);
    }

    public Task<int> CountAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        => _repository.CountAsync(includeInactive, cancellationToken);

    public async Task<ProjectDetailDto> CreateAsync(
        ProjectWriteDto dto,
        CancellationToken cancellationToken = default)
    {
        var project = new Project { CreatedDate = DateTime.UtcNow };
        dto.ApplyTo(project);

        await _repository.AddAsync(project, cancellationToken);
        InvalidateProjectCache();

        _logger.LogInformation("Project {ProjectId} '{Title}' created.", project.Id, project.Title);

        return project.ToDetailDto();
    }

    public async Task<ProjectDetailDto?> UpdateAsync(
        int id,
        ProjectWriteDto dto,
        CancellationToken cancellationToken = default)
    {
        // includeInactive so an admin can edit a project they have hidden.
        var project = await _repository.GetByIdAsync(id, includeImages: true,
            includeInactive: true, cancellationToken: cancellationToken);

        if (project is null)
        {
            return null;
        }

        dto.ApplyTo(project);
        project.UpdatedDate = DateTime.UtcNow;

        await _repository.UpdateAsync(project, cancellationToken);
        InvalidateProjectCache();

        _logger.LogInformation("Project {ProjectId} '{Title}' updated.", project.Id, project.Title);

        return project.ToDetailDto();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var project = await _repository.GetByIdAsync(id, includeImages: false,
            includeInactive: true, cancellationToken: cancellationToken);

        if (project is null)
        {
            return false;
        }

        await _repository.DeleteAsync(project, cancellationToken);
        InvalidateProjectCache();

        _logger.LogWarning("Project {ProjectId} '{Title}' deleted.", id, project.Title);

        return true;
    }

    public async Task<ProjectImageDto?> AddImageAsync(
        int projectId,
        string imageUrl,
        string? caption,
        CancellationToken cancellationToken = default)
    {
        var project = await _repository.GetByIdAsync(projectId, includeImages: true,
            includeInactive: true, cancellationToken: cancellationToken);

        if (project is null)
        {
            return null;
        }

        var image = new ProjectImage
        {
            ProjectId = projectId,
            ImageUrl = imageUrl,
            Caption = string.IsNullOrWhiteSpace(caption) ? null : caption.Trim(),
            // Append to the end of the existing gallery.
            SortOrder = project.Images.Count == 0 ? 0 : project.Images.Max(i => i.SortOrder) + 1
        };

        await _repository.AddImageAsync(image, cancellationToken);
        InvalidateProjectCache();

        return new ProjectImageDto
        {
            Id = image.Id,
            ImageUrl = image.ImageUrl,
            Caption = image.Caption,
            SortOrder = image.SortOrder
        };
    }

    public async Task<bool> DeleteImageAsync(int imageId, CancellationToken cancellationToken = default)
    {
        var image = await _repository.GetImageAsync(imageId, cancellationToken);

        if (image is null)
        {
            return false;
        }

        await _repository.DeleteImageAsync(image, cancellationToken);
        InvalidateProjectCache();

        return true;
    }

    /// <summary>
    /// Any project write invalidates the whole region: list pages, the featured
    /// strip, the category filter and the detail entry can all be affected by a
    /// single edit (README section 17).
    /// </summary>
    private void InvalidateProjectCache() => _cache.RemoveRegion(CacheRegions.Projects);
}
