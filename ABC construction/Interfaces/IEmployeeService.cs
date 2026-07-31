using ABC_construction.DTOs;

namespace ABC_construction.Interfaces;

/// <summary>Business operations for employees (README section 12).</summary>
public interface IEmployeeService
{
    Task<IReadOnlyList<EmployeeDto>> GetAllAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    Task<EmployeeDto?> GetByIdAsync(
        int id,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(bool includeInactive = false, CancellationToken cancellationToken = default);

    Task<EmployeeDto> CreateAsync(EmployeeWriteDto dto, CancellationToken cancellationToken = default);

    Task<EmployeeDto?> UpdateAsync(int id, EmployeeWriteDto dto, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
