using ABC_construction.Data;
using ABC_construction.Interfaces;
using ABC_construction.Models;
using Microsoft.EntityFrameworkCore;

namespace ABC_construction.Repositories;

/// <inheritdoc cref="IProjectRepository"/>
public class ProjectRepository : IProjectRepository
{
    private readonly ApplicationDbContext _context;

    public ProjectRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Project>> GetPagedAsync(
        int page,
        int pageSize,
        string? category = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = BaseQuery(includeInactive);

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category == category);
        }

        // Count before paging so TotalCount reflects the filtered set.
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await Newest(query)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new PagedResult<Project>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IReadOnlyList<Project>> GetFeaturedAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        return await Newest(BaseQuery(includeInactive: false))
            .Take(count)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Project?> GetByIdAsync(
        int id,
        bool includeImages = false,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = BaseQuery(includeInactive);

        if (includeImages)
        {
            query = query
                .Include(p => p.Images.OrderBy(i => i.SortOrder).ThenBy(i => i.Id));
        }

        // Tracked on purpose: admin edit flows load then mutate the entity.
        return await query.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Where(p => p.IsActive)
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<(string Name, string? NameKa)>> GetCategoryLabelsAsync(
        CancellationToken cancellationToken = default)
    {
        // Distinct pairs, then collapsed in memory: projects in one category
        // may disagree on (or omit) the Georgian label, and the list is tiny.
        var pairs = await _context.Projects
            .Where(p => p.IsActive)
            .Select(p => new { p.Category, p.CategoryKa })
            .Distinct()
            .ToListAsync(cancellationToken);

        return pairs
            .GroupBy(p => p.Category)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => (g.Key, g
                .Select(p => p.CategoryKa)
                .Where(ka => !string.IsNullOrWhiteSpace(ka))
                .Order(StringComparer.Ordinal)
                .FirstOrDefault()))
            .ToList();
    }

    public Task<int> CountAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        => BaseQuery(includeInactive).CountAsync(cancellationToken);

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        => _context.Projects.AnyAsync(p => p.Id == id, cancellationToken);

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        await _context.Projects.AddAsync(project, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Project project, CancellationToken cancellationToken = default)
    {
        _context.Projects.Remove(project);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddImageAsync(ProjectImage image, CancellationToken cancellationToken = default)
    {
        await _context.ProjectImages.AddAsync(image, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<ProjectImage?> GetImageAsync(int imageId, CancellationToken cancellationToken = default)
        => _context.ProjectImages.FirstOrDefaultAsync(i => i.Id == imageId, cancellationToken);

    public async Task DeleteImageAsync(ProjectImage image, CancellationToken cancellationToken = default)
    {
        _context.ProjectImages.Remove(image);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Applies the visibility filter every read shares, so a public caller can
    /// never accidentally surface a hidden project.
    /// </summary>
    private IQueryable<Project> BaseQuery(bool includeInactive)
    {
        IQueryable<Project> query = _context.Projects;
        return includeInactive ? query : query.Where(p => p.IsActive);
    }

    /// <summary>
    /// Most recently completed first, undated projects after all dated ones.
    /// The explicit HasValue key is needed because the providers disagree on
    /// where NULLs sort: PostgreSQL puts them first in a descending order,
    /// SQLite last.
    /// </summary>
    private static IOrderedQueryable<Project> Newest(IQueryable<Project> query) => query
        .OrderByDescending(p => p.CompletionDate.HasValue)
        .ThenByDescending(p => p.CompletionDate)
        .ThenByDescending(p => p.Id);
}
