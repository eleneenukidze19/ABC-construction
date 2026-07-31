using ABC_construction.Interfaces;
using ABC_construction.Mapping;
using ABC_construction.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ABC_construction.Controllers;

/// <summary>Public team page (README section 4).</summary>
public class EmployeesController : Controller
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet("/employees")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var employees = await _employeeService.GetAllAsync(
            includeInactive: false, cancellationToken);

        var model = new EmployeeListViewModel
        {
            Employees = employees.Select(e => e.ToViewModel()).ToList()
        };

        return View(model);
    }
}
