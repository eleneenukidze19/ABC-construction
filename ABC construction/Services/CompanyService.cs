using ABC_construction.Configuration;
using ABC_construction.DTOs;
using ABC_construction.Interfaces;
using ABC_construction.Mapping;
using ABC_construction.Models;
using Microsoft.Extensions.Options;

namespace ABC_construction.Services;

/// <inheritdoc cref="ICompanyService"/>
public class CompanyService : ICompanyService
{
    private const string CacheKey = "current";

    private readonly ICompanyInformationRepository _repository;
    private readonly ICacheService _cache;
    private readonly CachingOptions _cachingOptions;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(
        ICompanyInformationRepository repository,
        ICacheService cache,
        IOptions<CachingOptions> cachingOptions,
        ILogger<CompanyService> logger)
    {
        _repository = repository;
        _cache = cache;
        _cachingOptions = cachingOptions.Value;
        _logger = logger;
    }

    public async Task<CompanyInformationDto> GetAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheRegions.Company,
            CacheKey,
            async ct => (await _repository.GetAsync(ct)).ToDto(),
            _cachingOptions.CompanyTtl,
            cancellationToken);
    }

    public async Task<CompanyInformationDto> UpdateAsync(
        CompanyInformationWriteDto dto,
        CancellationToken cancellationToken = default)
    {
        var info = await _repository.GetAsync(cancellationToken);
        dto.ApplyTo(info);

        await _repository.UpdateAsync(info, cancellationToken);
        _cache.RemoveRegion(CacheRegions.Company);

        _logger.LogInformation("Company information updated.");

        return info.ToDto();
    }

    public async Task<bool> HasPlaceholderDetailsAsync(CancellationToken cancellationToken = default)
    {
        var info = await GetAsync(cancellationToken);

        return IsPlaceholder(info.Email)
            || IsPlaceholder(info.PhoneNumber)
            || IsPlaceholder(info.Address);
    }

    private static bool IsPlaceholder(string value) =>
        string.IsNullOrWhiteSpace(value)
        || string.Equals(value, CompanyInformation.Unwritten, StringComparison.OrdinalIgnoreCase);
}
