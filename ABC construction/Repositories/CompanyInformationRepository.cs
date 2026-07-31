using ABC_construction.Data;
using ABC_construction.Interfaces;
using ABC_construction.Models;
using Microsoft.EntityFrameworkCore;

namespace ABC_construction.Repositories;

/// <inheritdoc cref="ICompanyInformationRepository"/>
public class CompanyInformationRepository : ICompanyInformationRepository
{
    private readonly ApplicationDbContext _context;

    public CompanyInformationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyInformation> GetAsync(CancellationToken cancellationToken = default)
    {
        var info = await _context.CompanyInformation
            .FirstOrDefaultAsync(c => c.Id == Models.CompanyInformation.SingletonId, cancellationToken);

        if (info is not null)
        {
            return info;
        }

        // Defensive: the row is seeded by the migration, but recreate it rather
        // than letting /contact fail if the seed was removed by hand.
        info = new CompanyInformation { Id = Models.CompanyInformation.SingletonId };
        await _context.CompanyInformation.AddAsync(info, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return info;
    }

    public async Task UpdateAsync(CompanyInformation companyInformation, CancellationToken cancellationToken = default)
    {
        _context.CompanyInformation.Update(companyInformation);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
