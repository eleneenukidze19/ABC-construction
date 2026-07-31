using ABC_construction.Configuration;
using ABC_construction.Interfaces;
using Microsoft.Extensions.Options;

namespace ABC_construction.Services;

/// <inheritdoc cref="IFileUploadService"/>
/// <remarks>
/// Validation is deliberately layered (README sections 19 and 20): extension,
/// declared content type and the file's actual leading bytes must all agree.
/// Extension and content type are attacker-controlled, so the magic-number
/// check is what actually stops a script being uploaded as "photo.jpg".
/// </remarks>
public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _environment;
    private readonly FileUploadOptions _options;
    private readonly ILogger<FileUploadService> _logger;

    /// <summary>Leading bytes that must be present for each accepted extension.</summary>
    private static readonly Dictionary<string, byte[][]> MagicNumbers = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
        [".jpeg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
        [".png"] = new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
        // WEBP is a RIFF container: "RIFF" .... "WEBP" — bytes 8-11 checked separately.
        [".webp"] = new[] { new byte[] { 0x52, 0x49, 0x46, 0x46 } }
    };

    public FileUploadService(
        IWebHostEnvironment environment,
        IOptions<FileUploadOptions> options,
        ILogger<FileUploadService> logger)
    {
        _environment = environment;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<FileUploadResult> SaveImageAsync(
        IFormFile? file,
        string subFolder,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return FileUploadResult.Failure("No file was selected.");
        }

        if (file.Length > _options.MaxFileSizeBytes)
        {
            var maxMb = _options.MaxFileSizeBytes / 1024d / 1024d;
            return FileUploadResult.Failure($"File exceeds the {maxMb:0.#} MB limit.");
        }

        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();

        if (string.IsNullOrEmpty(extension)
            || !_options.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            var allowed = string.Join(", ", _options.AllowedExtensions);
            return FileUploadResult.Failure($"Unsupported file type. Allowed: {allowed}.");
        }

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return FileUploadResult.Failure("File must be an image.");
        }

        if (!await HasMatchingSignatureAsync(file, extension, cancellationToken))
        {
            _logger.LogWarning(
                "Rejected upload '{FileName}': content does not match extension {Extension}.",
                file.FileName, extension);

            return FileUploadResult.Failure("File contents do not match its extension.");
        }

        // The caller supplies subFolder, but treat it as untrusted anyway.
        var safeSubFolder = SanitiseSubFolder(subFolder);

        var targetDirectory = Path.Combine(
            _environment.WebRootPath,
            _options.UploadRootFolder.Replace('/', Path.DirectorySeparatorChar),
            safeSubFolder);

        Directory.CreateDirectory(targetDirectory);

        // The original filename is never reused — it is the primary vector for
        // path traversal and for overwriting existing files.
        var storedName = $"{Guid.NewGuid():N}{extension}";
        var absolutePath = Path.Combine(targetDirectory, storedName);

        await using (var stream = new FileStream(absolutePath, FileMode.CreateNew))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var relativeUrl = $"/{_options.UploadRootFolder.Trim('/')}/{safeSubFolder}/{storedName}";

        _logger.LogInformation(
            "Stored upload '{OriginalName}' ({Bytes} bytes) as {RelativeUrl}.",
            file.FileName, file.Length, relativeUrl);

        return FileUploadResult.Success(relativeUrl);
    }

    public Task DeleteImageAsync(string? relativeUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl))
        {
            return Task.CompletedTask;
        }

        var uploadRoot = Path.GetFullPath(Path.Combine(
            _environment.WebRootPath,
            _options.UploadRootFolder.Replace('/', Path.DirectorySeparatorChar)));

        var candidate = Path.GetFullPath(Path.Combine(
            _environment.WebRootPath,
            relativeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));

        // Refuse to delete anything that resolves outside the upload folder,
        // so a stored value like "../../appsettings.json" cannot be weaponised.
        if (!candidate.StartsWith(uploadRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Refused to delete {RelativeUrl}: outside the upload folder.", relativeUrl);
            return Task.CompletedTask;
        }

        try
        {
            if (File.Exists(candidate))
            {
                File.Delete(candidate);
                _logger.LogInformation("Deleted upload {RelativeUrl}.", relativeUrl);
            }
        }
        catch (IOException ex)
        {
            // A stale file on disk is not worth failing the admin's request over.
            _logger.LogError(ex, "Could not delete upload {RelativeUrl}.", relativeUrl);
        }

        return Task.CompletedTask;
    }

    private static async Task<bool> HasMatchingSignatureAsync(
        IFormFile file,
        string extension,
        CancellationToken cancellationToken)
    {
        if (!MagicNumbers.TryGetValue(extension, out var signatures))
        {
            return false;
        }

        // 12 bytes covers the longest check (WEBP's RIFF header).
        var header = new byte[12];

        await using var stream = file.OpenReadStream();
        var read = await stream.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, cancellationToken);

        var matchesSignature = signatures.Any(signature =>
            read >= signature.Length && header.Take(signature.Length).SequenceEqual(signature));

        if (!matchesSignature)
        {
            return false;
        }

        if (extension.Equals(".webp", StringComparison.OrdinalIgnoreCase))
        {
            // RIFF alone also matches WAV and AVI; require the WEBP form type.
            return read >= 12
                && header[8] == 0x57 && header[9] == 0x45
                && header[10] == 0x42 && header[11] == 0x50;
        }

        return true;
    }

    /// <summary>Reduces the folder name to a single safe path segment.</summary>
    private static string SanitiseSubFolder(string subFolder)
    {
        if (string.IsNullOrWhiteSpace(subFolder))
        {
            return "misc";
        }

        var cleaned = new string(subFolder
            .Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_')
            .ToArray());

        return string.IsNullOrEmpty(cleaned) ? "misc" : cleaned.ToLowerInvariant();
    }
}
