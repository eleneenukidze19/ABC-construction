namespace ABC_construction.Interfaces;

/// <summary>Outcome of an upload attempt (README section 20).</summary>
public record FileUploadResult(bool Succeeded, string? RelativeUrl, string? Error)
{
    public static FileUploadResult Success(string relativeUrl) => new(true, relativeUrl, null);
    public static FileUploadResult Failure(string error) => new(false, null, error);
}

/// <summary>
/// Validates and stores admin image uploads beneath wwwroot.
/// </summary>
public interface IFileUploadService
{
    /// <param name="subFolder">
    /// Grouping folder such as "projects" or "employees"; must not contain path
    /// separators.
    /// </param>
    Task<FileUploadResult> SaveImageAsync(
        IFormFile? file,
        string subFolder,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a previously uploaded file given the relative URL returned by
    /// <see cref="SaveImageAsync"/>. Ignores paths outside the upload folder.
    /// </summary>
    Task DeleteImageAsync(string? relativeUrl, CancellationToken cancellationToken = default);
}
