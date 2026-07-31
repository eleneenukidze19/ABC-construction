using ABC_construction.DTOs;

namespace ABC_construction.Interfaces;

/// <summary>
/// Management of admin panel accounts (README section 5: "Multiple users must
/// be able to have administrator access").
/// <para>
/// Wraps ASP.NET Core Identity's UserManager so controllers stay free of
/// business logic. The rules that matter live here, not in the UI: an
/// administrator must not be able to lock themselves out, and the site must
/// never be left without a working administrator.
/// </para>
/// </summary>
public interface IUserAdminService
{
    Task<IReadOnlyList<AdminUserDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<AdminUserDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<UserOperationResult> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default);

    /// <param name="currentUserId">
    /// The signed-in administrator, so self-demotion can be refused.
    /// </param>
    Task<UserOperationResult> UpdateAsync(
        string id,
        UpdateUserRequest request,
        string currentUserId,
        CancellationToken cancellationToken = default);

    Task<UserOperationResult> DeleteAsync(
        string id,
        string currentUserId,
        CancellationToken cancellationToken = default);

    /// <summary>Clears an active lockout after failed sign-in attempts.</summary>
    Task<UserOperationResult> UnlockAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Count of accounts currently holding the Admin role.</summary>
    Task<int> CountAdministratorsAsync(CancellationToken cancellationToken = default);
}
