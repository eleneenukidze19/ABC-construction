using ABC_construction.Models;

namespace ABC_construction.Interfaces;

/// <summary>
/// Data access for <see cref="Employee"/> (README section 11).
/// </summary>
public interface IEmployeeRepository
{
    Task<IReadOnlyList<Employee>> GetAllAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Employee>> GetPagedAsync(
        int page,
        int pageSize,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    Task<Employee?> GetByIdAsync(
        int id,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(bool includeInactive = false, CancellationToken cancellationToken = default);

    Task AddAsync(Employee employee, CancellationToken cancellationToken = default);

    Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default);

    Task DeleteAsync(Employee employee, CancellationToken cancellationToken = default);
}
