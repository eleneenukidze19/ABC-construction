namespace ABC_construction.Configuration;

/// <summary>Cache lifetimes, bound from the "Caching" configuration section.</summary>
public class CachingOptions
{
    public const string SectionName = "Caching";

    public int ProjectsAbsoluteExpirationMinutes { get; set; } = 10;
    public int EmployeesAbsoluteExpirationMinutes { get; set; } = 30;
    public int CompanyAbsoluteExpirationMinutes { get; set; } = 60;

    public TimeSpan ProjectsTtl => TimeSpan.FromMinutes(ProjectsAbsoluteExpirationMinutes);
    public TimeSpan EmployeesTtl => TimeSpan.FromMinutes(EmployeesAbsoluteExpirationMinutes);
    public TimeSpan CompanyTtl => TimeSpan.FromMinutes(CompanyAbsoluteExpirationMinutes);
}

/// <summary>Upload constraints, bound from the "FileUpload" section (README section 20).</summary>
public class FileUploadOptions
{
    public const string SectionName = "FileUpload";

    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;

    public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".webp" };

    /// <summary>Folder beneath wwwroot that uploads are written to.</summary>
    public string UploadRootFolder { get; set; } = "images/uploads";
}

/// <summary>Request limits, bound from the "RateLimiting" section (README section 18).</summary>
public class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public int AnonymousPermitPerMinute { get; set; } = 100;
    public int AuthenticatedPermitPerMinute { get; set; } = 300;
    public int QueueLimit { get; set; }
}

/// <summary>Initial admin account, bound from the "AdminSeed" section.</summary>
public class AdminSeedOptions
{
    public const string SectionName = "AdminSeed";

    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
