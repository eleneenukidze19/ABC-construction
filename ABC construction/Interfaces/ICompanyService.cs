using ABC_construction.DTOs;

namespace ABC_construction.Interfaces;

/// <summary>Business operations for company contact details (README section 12).</summary>
public interface ICompanyService
{
    Task<CompanyInformationDto> GetAsync(CancellationToken cancellationToken = default);

    Task<CompanyInformationDto> UpdateAsync(
        CompanyInformationWriteDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// True while any contact field is still the seeded "unwritten" placeholder.
    /// Surfaced on the admin dashboard as a prompt to complete setup.
    /// </summary>
    Task<bool> HasPlaceholderDetailsAsync(CancellationToken cancellationToken = default);
}
