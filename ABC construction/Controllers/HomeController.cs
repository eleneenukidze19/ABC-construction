using ABC_construction.Interfaces;
using ABC_construction.Mapping;
using ABC_construction.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ABC_construction.Controllers;

/// <summary>Landing page and error page (README section 4).</summary>
public class HomeController : Controller
{
    /// <summary>Year the company was founded, used for the "years in business" stat.</summary>
    private const int FoundedYear = 2009;

    private readonly IProjectService _projectService;
    private readonly IEmployeeService _employeeService;
    private readonly ICompanyService _companyService;

    public HomeController(
        IProjectService projectService,
        IEmployeeService employeeService,
        ICompanyService companyService)
    {
        _projectService = projectService;
        _employeeService = employeeService;
        _companyService = companyService;
    }

    [HttpGet("/")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var featured = await _projectService.GetFeaturedAsync(3, cancellationToken);
        var company = await _companyService.GetAsync(cancellationToken);

        var model = new HomeViewModel
        {
            FeaturedProjects = featured.Select(p => p.ToViewModel()).ToList(),
            Contact = company.ToViewModel(),
            CompletedProjectsCount = await _projectService.CountAsync(cancellationToken: cancellationToken),
            TeamSize = await _employeeService.CountAsync(cancellationToken: cancellationToken),
            YearsInBusiness = Math.Max(1, DateTime.UtcNow.Year - FoundedYear)
        };

        return View(model);
    }

    /// <summary>
    /// Target of the redirect issued by <c>ExceptionHandlingMiddleware</c>, and
    /// of status-code re-execution for 404s.
    /// </summary>
    /// <remarks>
    /// Routed for every verb, not just GET. UseStatusCodePagesWithReExecute
    /// replays the original request method against this path, so a failed POST
    /// would otherwise match nothing and surface a misleading 405 instead of
    /// the real status code.
    /// </remarks>
    [Route("/Home/Error")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Error(string? correlationId, int? statusCode)
    {
        ViewData["CorrelationId"] = correlationId ?? HttpContext.TraceIdentifier;
        ViewData["StatusCode"] = statusCode;

        Response.StatusCode = statusCode == 404
            ? StatusCodes.Status404NotFound
            : StatusCodes.Status500InternalServerError;

        return View("Error");
    }
}
