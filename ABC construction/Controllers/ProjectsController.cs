using ABC_construction.Interfaces;
using ABC_construction.Mapping;
using ABC_construction.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ABC_construction.Controllers;

/// <summary>Public project listing and detail pages (README section 4).</summary>
public class ProjectsController : Controller
{
    private const int PageSize = 9;

    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet("/projects")]
    public async Task<IActionResult> Index(
        [FromQuery] int page = 1,
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _projectService.GetPagedAsync(
            page, PageSize, category, includeInactive: false, cancellationToken);

        var model = new ProjectListViewModel
        {
            Projects = result.Items.Select(p => p.ToViewModel()).ToList(),
            Categories = (await _projectService.GetCategoryOptionsAsync(cancellationToken))
                .Select(c => c.ToOption())
                .ToList(),
            SelectedCategory = category,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages,
            TotalCount = result.TotalCount
        };

        return View(model);
    }

    [HttpGet("/projects/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var project = await _projectService.GetByIdAsync(id, includeInactive: false, cancellationToken);

        if (project is null)
        {
            // Renders the shared error view as a 404 rather than a bare status.
            return NotFound();
        }

        return View(project.ToDetailViewModel());
    }
}
