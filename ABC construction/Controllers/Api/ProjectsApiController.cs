using ABC_construction.DTOs;
using ABC_construction.Interfaces;
using ABC_construction.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABC_construction.Controllers.Api;

/// <summary>
/// REST endpoints for projects (README section 14). Holds no business logic —
/// every operation is delegated to <see cref="IProjectService"/>.
/// </summary>
[ApiController]
[Route("api/projects")]
[Produces("application/json")]
// The global AutoValidateAntiforgeryToken filter is for the admin panel's HTML
// forms; requiring a form token here would break every non-browser REST client.
// CSRF on these cookie-authenticated writes is instead blocked by the auth
// cookie's SameSite=Strict policy (see Program.cs).
[IgnoreAntiforgeryToken]
public class ProjectsApiController : ControllerBase
{
    private const int DefaultPageSize = 9;

    private readonly IProjectService _projectService;
    private readonly ILogger<ProjectsApiController> _logger;

    public ProjectsApiController(IProjectService projectService, ILogger<ProjectsApiController> logger)
    {
        _projectService = projectService;
        _logger = logger;
    }

    /// <summary>Gets projects, newest completion first.</summary>
    /// <remarks>
    /// Paged rather than unbounded (README section 23 requires pagination); pass
    /// page/pageSize to walk the full set.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ProjectSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ProjectSummaryDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _projectService.GetPagedAsync(
            page, pageSize, category, includeInactive: false, cancellationToken);

        return Ok(result);
    }

    /// <summary>Gets the distinct categories currently in use.</summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<string>>> GetCategories(
        CancellationToken cancellationToken = default)
    {
        return Ok(await _projectService.GetCategoriesAsync(cancellationToken));
    }

    /// <summary>Gets one project, including its gallery.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDetailDto>> GetById(
        int id,
        CancellationToken cancellationToken = default)
    {
        // Admins may inspect hidden projects; anonymous callers may not.
        var includeInactive = User.IsInRole(ApplicationRoles.Admin);

        var project = await _projectService.GetByIdAsync(id, includeInactive, cancellationToken);

        return project is null ? NotFound() : Ok(project);
    }

    /// <summary>Creates a project. Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = ApplicationRoles.Admin)]
    [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProjectDetailDto>> Create(
        [FromBody] ProjectWriteDto dto,
        CancellationToken cancellationToken = default)
    {
        var created = await _projectService.CreateAsync(dto, cancellationToken);

        _logger.LogInformation(
            "Admin {User} created project {ProjectId}.", User.Identity?.Name, created.Id);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates a project. Admin only.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDetailDto>> Update(
        int id,
        [FromBody] ProjectWriteDto dto,
        CancellationToken cancellationToken = default)
    {
        var updated = await _projectService.UpdateAsync(id, dto, cancellationToken);

        if (updated is null)
        {
            return NotFound();
        }

        _logger.LogInformation("Admin {User} updated project {ProjectId}.", User.Identity?.Name, id);

        return Ok(updated);
    }

    /// <summary>Deletes a project and its gallery. Admin only.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var deleted = await _projectService.DeleteAsync(id, cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        _logger.LogWarning("Admin {User} deleted project {ProjectId}.", User.Identity?.Name, id);

        return NoContent();
    }
}
