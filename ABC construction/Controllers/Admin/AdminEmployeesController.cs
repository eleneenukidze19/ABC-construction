using ABC_construction.DTOs;
using ABC_construction.Interfaces;
using ABC_construction.Models;
using ABC_construction.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABC_construction.Controllers.Admin;

/// <summary>Employee management screens (README section 6).</summary>
[Authorize(Roles = ApplicationRoles.Admin)]
[Route("admin/employees")]
public class AdminEmployeesController : Controller
{
    private const string UploadFolder = "employees";

    private readonly IEmployeeService _employeeService;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<AdminEmployeesController> _logger;

    public AdminEmployeesController(
        IEmployeeService employeeService,
        IFileUploadService fileUploadService,
        ILogger<AdminEmployeesController> logger)
    {
        _employeeService = employeeService;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var employees = await _employeeService.GetAllAsync(includeInactive: true, cancellationToken);

        var rows = employees.Select(e => new AdminEmployeeRowViewModel
        {
            Id = e.Id,
            FullName = e.FullName,
            Position = e.Position,
            SortOrder = e.SortOrder,
            ImageUrl = e.ImageUrl,
            IsActive = e.IsActive
        }).ToList();

        return View(rows);
    }

    [HttpGet("create")]
    public IActionResult Create() => View("Form", new EmployeeFormViewModel());

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        EmployeeFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var portraitUrl = await TryUploadAsync(model.Portrait, nameof(model.Portrait), cancellationToken);

        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var created = await _employeeService.CreateAsync(
            ToWriteDto(model, portraitUrl), cancellationToken);

        _logger.LogInformation(
            "Admin {User} created employee {EmployeeId}.", User.Identity?.Name, created.Id);

        TempData["Success"] = $"{created.FullName} added to the team.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var employee = await _employeeService.GetByIdAsync(id, includeInactive: true, cancellationToken);

        if (employee is null)
        {
            return NotFound();
        }

        return View("Form", new EmployeeFormViewModel
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Position = employee.Position,
            Biography = employee.Biography,
            SortOrder = employee.SortOrder,
            IsActive = employee.IsActive,
            ExistingImageUrl = employee.ImageUrl
        });
    }

    [HttpPost("edit/{id:int}")]
    public async Task<IActionResult> Edit(
        int id,
        EmployeeFormViewModel model,
        CancellationToken cancellationToken)
    {
        model.Id = id;

        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var uploadedUrl = await TryUploadAsync(model.Portrait, nameof(model.Portrait), cancellationToken);

        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var portraitUrl = uploadedUrl ?? model.ExistingImageUrl;

        var updated = await _employeeService.UpdateAsync(
            id, ToWriteDto(model, portraitUrl), cancellationToken);

        if (updated is null)
        {
            return NotFound();
        }

        if (uploadedUrl is not null
            && !string.IsNullOrWhiteSpace(model.ExistingImageUrl)
            && model.ExistingImageUrl != uploadedUrl)
        {
            await _fileUploadService.DeleteImageAsync(model.ExistingImageUrl, cancellationToken);
        }

        _logger.LogInformation("Admin {User} updated employee {EmployeeId}.", User.Identity?.Name, id);

        TempData["Success"] = $"{updated.FullName} saved.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var employee = await _employeeService.GetByIdAsync(id, includeInactive: true, cancellationToken);

        if (employee is null)
        {
            return NotFound();
        }

        if (await _employeeService.DeleteAsync(id, cancellationToken))
        {
            await _fileUploadService.DeleteImageAsync(employee.ImageUrl, cancellationToken);

            _logger.LogWarning("Admin {User} deleted employee {EmployeeId}.", User.Identity?.Name, id);

            TempData["Success"] = $"{employee.FullName} removed.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<string?> TryUploadAsync(
        IFormFile? file,
        string fieldName,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return null;
        }

        var result = await _fileUploadService.SaveImageAsync(file, UploadFolder, cancellationToken);

        if (result.Succeeded)
        {
            return result.RelativeUrl;
        }

        ModelState.AddModelError(fieldName, result.Error ?? "The image could not be uploaded.");
        return null;
    }

    private static EmployeeWriteDto ToWriteDto(EmployeeFormViewModel model, string? imageUrl) => new()
    {
        FullName = model.FullName,
        Position = model.Position,
        Biography = model.Biography,
        ImageUrl = imageUrl,
        SortOrder = model.SortOrder,
        IsActive = model.IsActive
    };
}
