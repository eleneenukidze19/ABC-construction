using ABC_construction.Configuration;
using ABC_construction.DTOs;
using ABC_construction.Interfaces;
using ABC_construction.Mapping;
using ABC_construction.Models;
using Microsoft.Extensions.Options;

namespace ABC_construction.Services;

/// <inheritdoc cref="IEmployeeService"/>
public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;
    private readonly ICacheService _cache;
    private readonly CachingOptions _cachingOptions;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(
        IEmployeeRepository repository,
        ICacheService cache,
        IOptions<CachingOptions> cachingOptions,
        ILogger<EmployeeService> logger)
    {
        _repository = repository;
        _cache = cache;
        _cachingOptions = cachingOptions.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<EmployeeDto>> GetAllAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        // Admin listings bypass the cache so writes are visible immediately.
        if (includeInactive)
        {
            var all = await _repository.GetAllAsync(includeInactive: true, cancellationToken);
            return all.Select(e => e.ToDto()).ToList();
        }

        return await _cache.GetOrCreateAsync(
            CacheRegions.Employees,
            "all",
            async ct =>
            {
                var employees = await _repository.GetAllAsync(false, ct);
                return (IReadOnlyList<EmployeeDto>)employees.Select(e => e.ToDto()).ToList();
            },
            _cachingOptions.EmployeesTtl,
            cancellationToken);
    }

    public async Task<EmployeeDto?> GetByIdAsync(
        int id,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var employee = await _repository.GetByIdAsync(id, includeInactive, cancellationToken);
        return employee?.ToDto();
    }

    public Task<int> CountAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        => _repository.CountAsync(includeInactive, cancellationToken);

    public async Task<EmployeeDto> CreateAsync(
        EmployeeWriteDto dto,
        CancellationToken cancellationToken = default)
    {
        var employee = new Employee { CreatedDate = DateTime.UtcNow };
        dto.ApplyTo(employee);

        await _repository.AddAsync(employee, cancellationToken);
        InvalidateEmployeeCache();

        _logger.LogInformation("Employee {EmployeeId} '{FullName}' created.", employee.Id, employee.FullName);

        return employee.ToDto();
    }

    public async Task<EmployeeDto?> UpdateAsync(
        int id,
        EmployeeWriteDto dto,
        CancellationToken cancellationToken = default)
    {
        var employee = await _repository.GetByIdAsync(id, includeInactive: true, cancellationToken);

        if (employee is null)
        {
            return null;
        }

        dto.ApplyTo(employee);

        await _repository.UpdateAsync(employee, cancellationToken);
        InvalidateEmployeeCache();

        _logger.LogInformation("Employee {EmployeeId} '{FullName}' updated.", employee.Id, employee.FullName);

        return employee.ToDto();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await _repository.GetByIdAsync(id, includeInactive: true, cancellationToken);

        if (employee is null)
        {
            return false;
        }

        await _repository.DeleteAsync(employee, cancellationToken);
        InvalidateEmployeeCache();

        _logger.LogWarning("Employee {EmployeeId} '{FullName}' deleted.", id, employee.FullName);

        return true;
    }

    private void InvalidateEmployeeCache() => _cache.RemoveRegion(CacheRegions.Employees);
}
