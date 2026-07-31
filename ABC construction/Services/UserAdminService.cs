using ABC_construction.DTOs;
using ABC_construction.Interfaces;
using ABC_construction.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ABC_construction.Services;

/// <inheritdoc cref="IUserAdminService"/>
public class UserAdminService : IUserAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserAdminService> _logger;

    public UserAdminService(
        UserManager<ApplicationUser> userManager,
        ILogger<UserAdminService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AdminUserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users
            .OrderBy(u => u.Email)
            .ToListAsync(cancellationToken);

        var result = new List<AdminUserDto>(users.Count);

        foreach (var user in users)
        {
            result.Add(await ToDtoAsync(user));
        }

        return result;
    }

    public async Task<AdminUserDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        return user is null ? null : await ToDtoAsync(user);
    }

    public async Task<UserOperationResult> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();

        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            return UserOperationResult.Failure("An account with that email address already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            // Accounts are created by an administrator who has verified the
            // person, so there is no confirmation email flow to complete.
            EmailConfirmed = true,
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? null : request.DisplayName.Trim(),
            CreatedDate = DateTime.UtcNow
        };

        var created = await _userManager.CreateAsync(user, request.Password);

        if (!created.Succeeded)
        {
            return UserOperationResult.Failure(created.Errors.Select(e => e.Description));
        }

        var roles = SanitiseRoles(request.Roles);

        if (roles.Count > 0)
        {
            var roleResult = await _userManager.AddToRolesAsync(user, roles);

            if (!roleResult.Succeeded)
            {
                // Roll back rather than leave an account that cannot do anything.
                await _userManager.DeleteAsync(user);
                return UserOperationResult.Failure(roleResult.Errors.Select(e => e.Description));
            }
        }

        _logger.LogInformation(
            "Created admin account {Email} with roles {Roles}.", email, string.Join(", ", roles));

        return UserOperationResult.Success();
    }

    public async Task<UserOperationResult> UpdateAsync(
        string id,
        UpdateUserRequest request,
        string currentUserId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            return UserOperationResult.Failure("That account no longer exists.");
        }

        var email = request.Email.Trim();

        // Identity requires unique emails; check before mutating anything.
        var existing = await _userManager.FindByEmailAsync(email);

        if (existing is not null && existing.Id != user.Id)
        {
            return UserOperationResult.Failure("Another account already uses that email address.");
        }

        var requestedRoles = SanitiseRoles(request.Roles);
        var currentRoles = await _userManager.GetRolesAsync(user);

        var losingAdmin = currentRoles.Contains(ApplicationRoles.Admin)
                          && !requestedRoles.Contains(ApplicationRoles.Admin);

        if (losingAdmin)
        {
            // An admin removing their own Admin role would immediately lose
            // access to this very screen.
            if (user.Id == currentUserId)
            {
                return UserOperationResult.Failure(
                    "You cannot remove your own administrator role. Ask another administrator to do it.");
            }

            if (await CountAdministratorsAsync(cancellationToken) <= 1)
            {
                return UserOperationResult.Failure(
                    "This is the only administrator account. Grant the role to someone else first.");
            }
        }

        user.Email = email;
        user.UserName = email;
        user.DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? null : request.DisplayName.Trim();

        var updated = await _userManager.UpdateAsync(user);

        if (!updated.Succeeded)
        {
            return UserOperationResult.Failure(updated.Errors.Select(e => e.Description));
        }

        var toRemove = currentRoles.Except(requestedRoles).ToList();
        var toAdd = requestedRoles.Except(currentRoles).ToList();

        if (toRemove.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, toRemove);

            if (!removeResult.Succeeded)
            {
                return UserOperationResult.Failure(removeResult.Errors.Select(e => e.Description));
            }
        }

        if (toAdd.Count > 0)
        {
            var addResult = await _userManager.AddToRolesAsync(user, toAdd);

            if (!addResult.Succeeded)
            {
                return UserOperationResult.Failure(addResult.Errors.Select(e => e.Description));
            }
        }

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            // Admin-initiated reset: remove the old hash and set a new one
            // rather than requiring the existing password.
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

            if (!passwordResult.Succeeded)
            {
                return UserOperationResult.Failure(passwordResult.Errors.Select(e => e.Description));
            }

            // Invalidate existing cookies for that account.
            await _userManager.UpdateSecurityStampAsync(user);

            _logger.LogWarning("Password reset for admin account {Email}.", user.Email);
        }

        _logger.LogInformation("Updated admin account {Email}.", user.Email);

        return UserOperationResult.Success();
    }

    public async Task<UserOperationResult> DeleteAsync(
        string id,
        string currentUserId,
        CancellationToken cancellationToken = default)
    {
        if (id == currentUserId)
        {
            return UserOperationResult.Failure("You cannot delete the account you are signed in with.");
        }

        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            return UserOperationResult.Failure("That account no longer exists.");
        }

        if (await _userManager.IsInRoleAsync(user, ApplicationRoles.Admin)
            && await CountAdministratorsAsync(cancellationToken) <= 1)
        {
            return UserOperationResult.Failure(
                "This is the only administrator account and cannot be deleted.");
        }

        var deleted = await _userManager.DeleteAsync(user);

        if (!deleted.Succeeded)
        {
            return UserOperationResult.Failure(deleted.Errors.Select(e => e.Description));
        }

        _logger.LogWarning("Deleted admin account {Email}.", user.Email);

        return UserOperationResult.Success();
    }

    public async Task<UserOperationResult> UnlockAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            return UserOperationResult.Failure("That account no longer exists.");
        }

        var result = await _userManager.SetLockoutEndDateAsync(user, null);

        if (!result.Succeeded)
        {
            return UserOperationResult.Failure(result.Errors.Select(e => e.Description));
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        _logger.LogInformation("Cleared lockout for admin account {Email}.", user.Email);

        return UserOperationResult.Success();
    }

    public async Task<int> CountAdministratorsAsync(CancellationToken cancellationToken = default)
    {
        var admins = await _userManager.GetUsersInRoleAsync(ApplicationRoles.Admin);
        return admins.Count;
    }

    private async Task<AdminUserDto> ToDtoAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new AdminUserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            DisplayName = user.DisplayName,
            Roles = roles.OrderBy(r => r).ToList(),
            CreatedDate = user.CreatedDate,
            // LockoutEnd sits in the past once a lockout has expired, so compare
            // against now rather than treating any value as "locked".
            IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow,
            LockoutEnd = user.LockoutEnd
        };
    }

    /// <summary>
    /// Drops anything not in <see cref="ApplicationRoles.All"/>, so a tampered
    /// form cannot grant a role the application does not define.
    /// </summary>
    private static List<string> SanitiseRoles(IReadOnlyList<string> roles) =>
        roles.Where(r => ApplicationRoles.All.Contains(r, StringComparer.Ordinal))
             .Distinct(StringComparer.Ordinal)
             .ToList();
}
