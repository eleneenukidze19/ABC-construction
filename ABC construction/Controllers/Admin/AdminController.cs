using ABC_construction.DTOs;
using ABC_construction.Interfaces;
using ABC_construction.Mapping;
using ABC_construction.Models;
using ABC_construction.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABC_construction.Controllers.Admin;

/// <summary>
/// Admin dashboard and company settings (README section 6).
/// </summary>
[Authorize(Roles = ApplicationRoles.Admin)]
[Route("admin")]
public class AdminController : Controller
{
    private readonly IProjectService _projectService;
    private readonly IEmployeeService _employeeService;
    private readonly ICompanyService _companyService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IProjectService projectService,
        IEmployeeService employeeService,
        ICompanyService companyService,
        ILogger<AdminController> logger)
    {
        _projectService = projectService;
        _employeeService = employeeService;
        _companyService = companyService;
        _logger = logger;
    }

    /// <summary>/admin lands on the dashboard.</summary>
    [HttpGet("")]
    public IActionResult Index() => RedirectToAction(nameof(Dashboard));

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var recent = await _projectService.GetPagedAsync(
            page: 1, pageSize: 5, category: null, includeInactive: true, cancellationToken);

        var model = new AdminDashboardViewModel
        {
            TotalProjects = await _projectService.CountAsync(includeInactive: true, cancellationToken),
            ActiveProjects = await _projectService.CountAsync(includeInactive: false, cancellationToken),
            TotalEmployees = await _employeeService.CountAsync(includeInactive: true, cancellationToken),
            ActiveEmployees = await _employeeService.CountAsync(includeInactive: false, cancellationToken),
            ContactDetailsIncomplete = await _companyService.HasPlaceholderDetailsAsync(cancellationToken),
            RecentProjects = recent.Items.Select(p => p.ToViewModel()).ToList()
        };

        return View(model);
    }

    [HttpGet("settings")]
    public async Task<IActionResult> Settings(CancellationToken cancellationToken)
    {
        var company = await _companyService.GetAsync(cancellationToken);

        return View(new CompanySettingsViewModel
        {
            Email = company.Email,
            PhoneNumber = company.PhoneNumber,
            Address = company.Address,
            Description = company.Description
        });
    }

    [HttpPost("settings")]
    public async Task<IActionResult> Settings(
        CompanySettingsViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _companyService.UpdateAsync(new CompanyInformationWriteDto
        {
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            Address = model.Address,
            Description = model.Description
        }, cancellationToken);

        _logger.LogInformation("Admin {User} updated company settings.", User.Identity?.Name);

        TempData["Success"] = "Company details updated. They are now live on the contact page.";

        // Redirect after POST so a refresh cannot resubmit the form.
        return RedirectToAction(nameof(Settings));
    }
}
