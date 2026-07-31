using ABC_construction.DTOs;
using ABC_construction.Interfaces;
using ABC_construction.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABC_construction.Controllers.Api;

/// <summary>REST endpoints for company contact details (README section 14).</summary>
[ApiController]
[Route("api/company")]
[Produces("application/json")]
// See ProjectsController: form anti-forgery does not apply to the REST surface;
// SameSite=Strict on the auth cookie is what blocks CSRF here.
[IgnoreAntiforgeryToken]
public class CompanyApiController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly ILogger<CompanyApiController> _logger;

    public CompanyApiController(ICompanyService companyService, ILogger<CompanyApiController> logger)
    {
        _companyService = companyService;
        _logger = logger;
    }

    /// <summary>Gets the published contact details.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(CompanyInformationDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CompanyInformationDto>> Get(
        CancellationToken cancellationToken = default)
    {
        return Ok(await _companyService.GetAsync(cancellationToken));
    }

    /// <summary>Updates the contact details. Admin only.</summary>
    [HttpPut]
    [Authorize(Roles = ApplicationRoles.Admin)]
    [ProducesResponseType(typeof(CompanyInformationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CompanyInformationDto>> Update(
        [FromBody] CompanyInformationWriteDto dto,
        CancellationToken cancellationToken = default)
    {
        var updated = await _companyService.UpdateAsync(dto, cancellationToken);

        _logger.LogInformation("Admin {User} updated company information.", User.Identity?.Name);

        return Ok(updated);
    }
}
