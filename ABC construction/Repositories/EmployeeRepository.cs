using ABC_construction.Data;
using ABC_construction.Interfaces;
using ABC_construction.Models;
using Microsoft.EntityFrameworkCore;

namespace ABC_construction.Repositories;

/// <inheritdoc cref="IEmployeeRepository"/>
public class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Employee>> GetAllAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        return await BaseQuery(includeInactive)
            .OrderBy(e => e.SortOrder)
            .ThenBy(e => e.FullName)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<Employee>> GetPagedAsync(
        int page,
        int pageSize,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = BaseQuery(includeInactive);
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(e => e.SortOrder)
            .ThenBy(e => e.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new PagedResult<Employee>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public Task<Employee?> GetByIdAsync(
        int id,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        // Tracked on purpose: admin edit flows load then mutate the entity.
        return BaseQuery(includeInactive).FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public Task<int> CountAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        => BaseQuery(includeInactive).CountAsync(cancellationToken);

    public async Task AddAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        await _context.Employees.AddAsync(employee, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        _context.Employees.Update(employee);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Employee> BaseQuery(bool includeInactive)
    {
        IQueryable<Employee> query = _context.Employees;
        return includeInactive ? query : query.Where(e => e.IsActive);
    }
}
