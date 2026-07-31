using System.Security.Claims;
using ABC_construction.DTOs;
using ABC_construction.Interfaces;
using ABC_construction.Models;
using ABC_construction.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABC_construction.Controllers.Admin;

/// <summary>
/// Administrator account management (README section 5). All rules about who
/// may be demoted or deleted live in <see cref="IUserAdminService"/>; this
/// controller only translates between forms and the service.
/// </summary>
[Authorize(Roles = ApplicationRoles.Admin)]
[Route("admin/users")]
public class AdminUsersController : Controller
{
    private readonly IUserAdminService _userAdminService;

    public AdminUsersController(IUserAdminService userAdminService)
    {
        _userAdminService = userAdminService;
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var users = await _userAdminService.GetAllAsync(cancellationToken);

        var model = new AdminUserListViewModel
        {
            AdministratorCount = await _userAdminService.CountAdministratorsAsync(cancellationToken),
            Users = users.Select(u => new AdminUserRowViewModel
            {
                Id = u.Id,
                Email = u.Email,
                DisplayName = u.DisplayName,
                Roles = u.Roles,
                CreatedDate = u.CreatedDate,
                IsLockedOut = u.IsLockedOut,
                IsCurrentUser = u.Id == CurrentUserId,
                IsProtected = u.IsProtected
            }).ToList()
        };

        return View(model);
    }

    [HttpGet("create")]
    public IActionResult Create() =>
        View(new CreateUserViewModel { SelectedRoles = { ApplicationRoles.Admin } });

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        CreateUserViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _userAdminService.CreateAsync(new CreateUserRequest
        {
            Email = model.Email,
            DisplayName = model.DisplayName,
            Password = model.Password,
            Roles = model.SelectedRoles
        }, cancellationToken);

        if (!result.Succeeded)
        {
            AddErrors(result);
            return View(model);
        }

        TempData["Success"] = $"Account created for {model.Email}.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken)
    {
        var user = await _userAdminService.GetByIdAsync(id, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        var adminCount = await _userAdminService.CountAdministratorsAsync(cancellationToken);

        return View(new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            SelectedRoles = user.Roles.ToList(),
            IsCurrentUser = user.Id == CurrentUserId,
            IsLockedOut = user.IsLockedOut,
            LockoutEnd = user.LockoutEnd,
            IsOnlyAdministrator = adminCount <= 1 && user.Roles.Contains(ApplicationRoles.Admin),
            IsProtected = user.IsProtected
        });
    }

    [HttpPost("edit/{id}")]
    public async Task<IActionResult> Edit(
        string id,
        EditUserViewModel model,
        CancellationToken cancellationToken)
    {
        model.Id = id;

        if (!ModelState.IsValid)
        {
            await RepopulateAsync(model, cancellationToken);
            return View(model);
        }

        var result = await _userAdminService.UpdateAsync(id, new UpdateUserRequest
        {
            Email = model.Email,
            DisplayName = model.DisplayName,
            Roles = model.SelectedRoles,
            NewPassword = model.NewPassword
        }, CurrentUserId, cancellationToken);

        if (!result.Succeeded)
        {
            AddErrors(result);
            await RepopulateAsync(model, cancellationToken);
            return View(model);
        }

        TempData["Success"] = string.IsNullOrWhiteSpace(model.NewPassword)
            ? $"Account {model.Email} updated."
            : $"Account {model.Email} updated and its password reset.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var user = await _userAdminService.GetByIdAsync(id, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        var result = await _userAdminService.DeleteAsync(id, CurrentUserId, cancellationToken);

        if (result.Succeeded)
        {
            TempData["Success"] = $"Account {user.Email} deleted.";
        }
        else
        {
            TempData["Error"] = string.Join(" ", result.Errors);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("unlock/{id}")]
    public async Task<IActionResult> Unlock(string id, CancellationToken cancellationToken)
    {
        var result = await _userAdminService.UnlockAsync(id, cancellationToken);

        if (result.Succeeded)
        {
            TempData["Success"] = "Account unlocked. They can sign in again now.";
        }
        else
        {
            TempData["Error"] = string.Join(" ", result.Errors);
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Restores the flags the form does not post back, so a redisplayed page
    /// still shows lockout state and the only-administrator guard correctly.
    /// </summary>
    private async Task RepopulateAsync(EditUserViewModel model, CancellationToken cancellationToken)
    {
        var user = await _userAdminService.GetByIdAsync(model.Id, cancellationToken);
        var adminCount = await _userAdminService.CountAdministratorsAsync(cancellationToken);

        model.IsCurrentUser = model.Id == CurrentUserId;
        model.IsLockedOut = user?.IsLockedOut ?? false;
        model.LockoutEnd = user?.LockoutEnd;
        model.IsOnlyAdministrator =
            adminCount <= 1 && (user?.Roles.Contains(ApplicationRoles.Admin) ?? false);
        model.IsProtected = user?.IsProtected ?? false;
    }

    private void AddErrors(UserOperationResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }
    }
}
