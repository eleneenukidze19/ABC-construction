using ABC_construction.Interfaces;
using ABC_construction.Mapping;
using Microsoft.AspNetCore.Mvc;

namespace ABC_construction.Controllers;

/// <summary>Public contact page (README section 4).</summary>
public class ContactController : Controller
{
    private readonly ICompanyService _companyService;

    public ContactController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpGet("/contact")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var company = await _companyService.GetAsync(cancellationToken);
        return View(company.ToViewModel());
    }
}
