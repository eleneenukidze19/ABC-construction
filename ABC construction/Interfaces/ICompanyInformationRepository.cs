using ABC_construction.Models;

namespace ABC_construction.Interfaces;

/// <summary>
/// Data access for the single <see cref="CompanyInformation"/> row
/// (README section 11).
/// </summary>
public interface ICompanyInformationRepository
{
    /// <summary>
    /// Returns the singleton row, creating it with the "unwritten" placeholders
    /// if the seed is missing, so callers never have to handle null.
    /// </summary>
    Task<CompanyInformation> GetAsync(CancellationToken cancellationToken = default);

    Task UpdateAsync(CompanyInformation companyInformation, CancellationToken cancellationToken = default);
}
