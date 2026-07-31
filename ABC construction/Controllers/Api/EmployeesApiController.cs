using ABC_construction.DTOs;
using ABC_construction.Interfaces;
using ABC_construction.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABC_construction.Controllers.Api;

/// <summary>REST endpoints for employees (README section 14).</summary>
[ApiController]
[Route("api/employees")]
[Produces("application/json")]
// See ProjectsController: form anti-forgery does not apply to the REST surface;
// SameSite=Strict on the auth cookie is what blocks CSRF here.
[IgnoreAntiforgeryToken]
public class EmployeesApiController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeesApiController> _logger;

    public EmployeesApiController(IEmployeeService employeeService, ILogger<EmployeesApiController> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    /// <summary>Gets the team, ordered for display.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EmployeeDto>>> GetAll(
        CancellationToken cancellationToken = default)
    {
        var includeInactive = User.IsInRole(ApplicationRoles.Admin);
        return Ok(await _employeeService.GetAllAsync(includeInactive, cancellationToken));
    }

    /// <summary>Gets one employee.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeDto>> GetById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var includeInactive = User.IsInRole(ApplicationRoles.Admin);
        var employee = await _employeeService.GetByIdAsync(id, includeInactive, cancellationToken);

        return employee is null ? NotFound() : Ok(employee);
    }

    /// <summary>Creates an employee. Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = ApplicationRoles.Admin)]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmployeeDto>> Create(
        [FromBody] EmployeeWriteDto dto,
        CancellationToken cancellationToken = default)
    {
        var created = await _employeeService.CreateAsync(dto, cancellationToken);

        _logger.LogInformation(
            "Admin {User} created employee {EmployeeId}.", User.Identity?.Name, created.Id);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an employee. Admin only.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeDto>> Update(
        int id,
        [FromBody] EmployeeWriteDto dto,
        CancellationToken cancellationToken = default)
    {
        var updated = await _employeeService.UpdateAsync(id, dto, cancellationToken);

        if (updated is null)
        {
            return NotFound();
        }

        _logger.LogInformation("Admin {User} updated employee {EmployeeId}.", User.Identity?.Name, id);

        return Ok(updated);
    }

    /// <summary>Deletes an employee. Admin only.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var deleted = await _employeeService.DeleteAsync(id, cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        _logger.LogWarning("Admin {User} deleted employee {EmployeeId}.", User.Identity?.Name, id);

        return NoContent();
    }
}
