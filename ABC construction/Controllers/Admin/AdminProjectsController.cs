using ABC_construction.DTOs;
using ABC_construction.Interfaces;
using ABC_construction.Models;
using ABC_construction.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABC_construction.Controllers.Admin;

/// <summary>
/// Project management screens (README section 6). All persistence goes through
/// <see cref="IProjectService"/>; uploads go through <see cref="IFileUploadService"/>.
/// </summary>
[Authorize(Roles = ApplicationRoles.Admin)]
[Route("admin/projects")]
public class AdminProjectsController : Controller
{
    private const int PageSize = 20;
    private const string UploadFolder = "projects";

    private readonly IProjectService _projectService;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<AdminProjectsController> _logger;

    public AdminProjectsController(
        IProjectService projectService,
        IFileUploadService fileUploadService,
        ILogger<AdminProjectsController> logger)
    {
        _projectService = projectService;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromQuery] int page = 1,
        CancellationToken cancellationToken = default)
    {
        var result = await _projectService.GetPagedAsync(
            page, PageSize, category: null, includeInactive: true, cancellationToken);

        ViewData["Page"] = result.Page;
        ViewData["TotalPages"] = result.TotalPages;

        var rows = result.Items.Select(p => new AdminProjectRowViewModel
        {
            Id = p.Id,
            Title = p.Title,
            Category = p.Category,
            Duration = p.Duration,
            CompletionDate = p.CompletionDate,
            ImageUrl = p.ImageUrl,
            IsActive = p.IsActive
        }).ToList();

        return View(rows);
    }

    [HttpGet("create")]
    public IActionResult Create() => View("Form", new ProjectFormViewModel());

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        ProjectFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var coverUrl = await TryUploadAsync(model.CoverImage, nameof(model.CoverImage), cancellationToken);

        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var created = await _projectService.CreateAsync(ToWriteDto(model, coverUrl), cancellationToken);

        _logger.LogInformation(
            "Admin {User} created project {ProjectId}.", User.Identity?.Name, created.Id);

        TempData["Success"] = $"Project “{created.Title}” created.";

        // Straight to edit so the admin can add gallery images next.
        return RedirectToAction(nameof(Edit), new { id = created.Id });
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var project = await _projectService.GetByIdAsync(id, includeInactive: true, cancellationToken);

        if (project is null)
        {
            return NotFound();
        }

        return View("Form", new ProjectFormViewModel
        {
            Id = project.Id,
            Title = project.Title,
            Category = project.Category,
            ShortDescription = project.ShortDescription,
            Description = project.Description,
            Duration = project.Duration,
            CompletionDate = project.CompletionDate,
            Timeline = project.Timeline,
            MaterialsUsed = project.MaterialsUsed,
            Challenges = project.Challenges,
            TitleKa = project.TitleKa,
            CategoryKa = project.CategoryKa,
            ShortDescriptionKa = project.ShortDescriptionKa,
            DescriptionKa = project.DescriptionKa,
            DurationKa = project.DurationKa,
            TimelineKa = project.TimelineKa,
            MaterialsUsedKa = project.MaterialsUsedKa,
            ChallengesKa = project.ChallengesKa,
            IsActive = project.IsActive,
            ExistingImageUrl = project.ImageUrl,
            Images = project.Images
                .Select(i => new ProjectImageViewModel
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    Caption = i.Caption
                })
                .ToList()
        });
    }

    [HttpPost("edit/{id:int}")]
    public async Task<IActionResult> Edit(
        int id,
        ProjectFormViewModel model,
        CancellationToken cancellationToken)
    {
        model.Id = id;

        if (!ModelState.IsValid)
        {
            await RepopulateGalleryAsync(model, cancellationToken);
            return View("Form", model);
        }

        var uploadedUrl = await TryUploadAsync(model.CoverImage, nameof(model.CoverImage), cancellationToken);

        if (!ModelState.IsValid)
        {
            await RepopulateGalleryAsync(model, cancellationToken);
            return View("Form", model);
        }

        // Keep the current cover when no replacement was supplied.
        var coverUrl = uploadedUrl ?? model.ExistingImageUrl;

        var updated = await _projectService.UpdateAsync(id, ToWriteDto(model, coverUrl), cancellationToken);

        if (updated is null)
        {
            return NotFound();
        }

        // Only remove the old file once the new record is safely saved.
        if (uploadedUrl is not null
            && !string.IsNullOrWhiteSpace(model.ExistingImageUrl)
            && model.ExistingImageUrl != uploadedUrl)
        {
            await _fileUploadService.DeleteImageAsync(model.ExistingImageUrl, cancellationToken);
        }

        _logger.LogInformation("Admin {User} updated project {ProjectId}.", User.Identity?.Name, id);

        TempData["Success"] = $"Project “{updated.Title}” saved.";

        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost("delete/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var project = await _projectService.GetByIdAsync(id, includeInactive: true, cancellationToken);

        if (project is null)
        {
            return NotFound();
        }

        var deleted = await _projectService.DeleteAsync(id, cancellationToken);

        if (deleted)
        {
            // Gallery rows cascade in the database; their files do not.
            await _fileUploadService.DeleteImageAsync(project.ImageUrl, cancellationToken);

            foreach (var image in project.Images)
            {
                await _fileUploadService.DeleteImageAsync(image.ImageUrl, cancellationToken);
            }

            _logger.LogWarning("Admin {User} deleted project {ProjectId}.", User.Identity?.Name, id);

            TempData["Success"] = $"Project “{project.Title}” deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:int}/images")]
    public async Task<IActionResult> AddImage(
        int id,
        ProjectFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (model.GalleryImage is null)
        {
            TempData["Error"] = "Choose an image file before adding it to the gallery.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        var result = await _fileUploadService.SaveImageAsync(
            model.GalleryImage, UploadFolder, cancellationToken);

        if (!result.Succeeded)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Edit), new { id });
        }

        var added = await _projectService.AddImageAsync(
            id, result.RelativeUrl!, model.GalleryCaption, cancellationToken);

        if (added is null)
        {
            // Project vanished between upload and save; do not orphan the file.
            await _fileUploadService.DeleteImageAsync(result.RelativeUrl, cancellationToken);
            return NotFound();
        }

        TempData["Success"] = "Gallery image added.";

        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost("{id:int}/images/{imageId:int}/delete")]
    public async Task<IActionResult> DeleteImage(
        int id,
        int imageId,
        CancellationToken cancellationToken)
    {
        var project = await _projectService.GetByIdAsync(id, includeInactive: true, cancellationToken);
        var image = project?.Images.FirstOrDefault(i => i.Id == imageId);

        if (image is null)
        {
            return NotFound();
        }

        if (await _projectService.DeleteImageAsync(imageId, cancellationToken))
        {
            await _fileUploadService.DeleteImageAsync(image.ImageUrl, cancellationToken);
            TempData["Success"] = "Gallery image removed.";
        }

        return RedirectToAction(nameof(Edit), new { id });
    }

    /// <summary>
    /// Saves an upload if one was supplied, adding a field-level model error on
    /// failure so the message renders beside the input rather than as a banner.
    /// </summary>
    private async Task<string?> TryUploadAsync(
        IFormFile? file,
        string fieldName,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return null;
        }

        var result = await _fileUploadService.SaveImageAsync(file, UploadFolder, cancellationToken);

        if (result.Succeeded)
        {
            return result.RelativeUrl;
        }

        ModelState.AddModelError(fieldName, result.Error ?? "The image could not be uploaded.");
        return null;
    }

    /// <summary>Restores the gallery list when redisplaying the form after an error.</summary>
    private async Task RepopulateGalleryAsync(ProjectFormViewModel model, CancellationToken cancellationToken)
    {
        if (!model.IsEdit)
        {
            return;
        }

        var project = await _projectService.GetByIdAsync(model.Id, includeInactive: true, cancellationToken);

        if (project is null)
        {
            return;
        }

        model.ExistingImageUrl ??= project.ImageUrl;
        model.Images = project.Images
            .Select(i => new ProjectImageViewModel
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                Caption = i.Caption
            })
            .ToList();
    }

    private static ProjectWriteDto ToWriteDto(ProjectFormViewModel model, string? imageUrl) => new()
    {
        Title = model.Title,
        Description = model.Description,
        ShortDescription = model.ShortDescription,
        Duration = model.Duration,
        CompletionDate = model.CompletionDate,
        Category = model.Category,
        ImageUrl = imageUrl,
        Timeline = model.Timeline,
        MaterialsUsed = model.MaterialsUsed,
        Challenges = model.Challenges,
        TitleKa = model.TitleKa,
        CategoryKa = model.CategoryKa,
        ShortDescriptionKa = model.ShortDescriptionKa,
        DescriptionKa = model.DescriptionKa,
        DurationKa = model.DurationKa,
        TimelineKa = model.TimelineKa,
        MaterialsUsedKa = model.MaterialsUsedKa,
        ChallengesKa = model.ChallengesKa,
        IsActive = model.IsActive
    };
}
