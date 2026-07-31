namespace ABC_construction.DTOs;

/// <summary>An admin panel account as shown in the user management screens.</summary>
public class AdminUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
    public DateTime CreatedDate { get; set; }

    /// <summary>True while an active lockout is in force.</summary>
    public bool IsLockedOut { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    /// <summary>
    /// True for the primary administrator (AdminSeed:Email), which cannot be
    /// deleted, demoted, or renamed.
    /// </summary>
    public bool IsProtected { get; set; }
}

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string Password { get; set; } = string.Empty;
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
}

public class UpdateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();

    /// <summary>When set, replaces the account password. Ignored when blank.</summary>
    public string? NewPassword { get; set; }
}

/// <summary>
/// Outcome of a user management operation. Carries messages rather than
/// throwing, so the controller can surface them beside the relevant field.
/// </summary>
public class UserOperationResult
{
    public bool Succeeded { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public static UserOperationResult Success() => new() { Succeeded = true };

    public static UserOperationResult Failure(params string[] errors) =>
        new() { Succeeded = false, Errors = errors };

    public static UserOperationResult Failure(IEnumerable<string> errors) =>
        new() { Succeeded = false, Errors = errors.ToList() };
}
