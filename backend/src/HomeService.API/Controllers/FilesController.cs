using HomeService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeService.API.Controllers;

[Authorize]
public class FilesController : BaseApiController
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<FilesController> _logger;

    // Allowed file extensions by category
    private static readonly Dictionary<string, string[]> AllowedExtensions = new()
    {
        ["images"] = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" },
        ["documents"] = new[] { ".pdf", ".doc", ".docx", ".txt" },
        ["videos"] = new[] { ".mp4", ".mov", ".avi", ".mkv" }
    };

    // Maximum file sizes by category (in bytes)
    private static readonly Dictionary<string, long> MaxFileSizes = new()
    {
        ["images"] = 5 * 1024 * 1024,      // 5 MB
        ["documents"] = 10 * 1024 * 1024,  // 10 MB
        ["videos"] = 100 * 1024 * 1024     // 100 MB
    };

    public FilesController(IFileStorageService fileStorageService, ILogger<FilesController> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Upload profile image
    /// </summary>
    [HttpPost("profile-image")]
    public async Task<IActionResult> UploadProfileImage(IFormFile file)
    {
        return await UploadFile(file, "profile-images", "images");
    }

    /// <summary>
    /// Upload booking/service completion photos
    /// </summary>
    [HttpPost("booking-photos")]
    public async Task<IActionResult> UploadBookingPhotos([FromForm] List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
            return BadRequest(new { message = "No files provided" });

        if (files.Count > 10)
            return BadRequest(new { message = "Maximum 10 files allowed" });

        var uploadedUrls = new List<string>();
        var errors = new List<string>();

        foreach (var file in files)
        {
            try
            {
                var result = await UploadFileInternal(file, "booking-photos", "images");
                if (result.Success)
                    uploadedUrls.Add(result.Url!);
                else
                    errors.Add($"{file.FileName}: {result.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
                errors.Add($"{file.FileName}: {ex.Message}");
            }
        }

        return Ok(new
        {
            uploadedFiles = uploadedUrls,
            errors = errors.Count > 0 ? errors : null,
            message = $"{uploadedUrls.Count} of {files.Count} files uploaded successfully"
        });
    }

    /// <summary>
    /// Upload document (certificates, IDs, etc.)
    /// </summary>
    [HttpPost("documents")]
    public async Task<IActionResult> UploadDocument(IFormFile file, [FromQuery] string? category = null)
    {
        var folder = string.IsNullOrEmpty(category) ? "documents" : $"documents/{category}";
        return await UploadFile(file, folder, "documents");
    }

    /// <summary>
    /// Upload service category image
    /// </summary>
    [HttpPost("category-images")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadCategoryImage(IFormFile file)
    {
        return await UploadFile(file, "category-images", "images");
    }

    /// <summary>
    /// Upload chat message attachment
    /// </summary>
    [HttpPost("chat-attachments")]
    public async Task<IActionResult> UploadChatAttachment(IFormFile file)
    {
        // Determine category based on file type
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        string category;

        if (AllowedExtensions["images"].Contains(extension))
            category = "images";
        else if (AllowedExtensions["documents"].Contains(extension))
            category = "documents";
        else if (AllowedExtensions["videos"].Contains(extension))
            category = "videos";
        else
            return BadRequest(new { message = "File type not allowed" });

        return await UploadFile(file, "chat-attachments", category);
    }

    /// <summary>
    /// Delete a file
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> DeleteFile([FromQuery] string fileUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(fileUrl))
                return BadRequest(new { message = "File URL is required" });

            var success = await _fileStorageService.DeleteFileAsync(fileUrl);

            if (!success)
                return NotFound(new { message = "File not found or already deleted" });

            _logger.LogInformation("File deleted: {FileUrl}", fileUrl);

            return Ok(new { message = "File deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileUrl}", fileUrl);
            return StatusCode(500, new { message = "Error deleting file", error = ex.Message });
        }
    }

    /// <summary>
    /// Get a temporary download URL for a file
    /// </summary>
    [HttpGet("download-url")]
    public async Task<IActionResult> GetDownloadUrl([FromQuery] string fileUrl, [FromQuery] int expiryMinutes = 60)
    {
        try
        {
            if (string.IsNullOrEmpty(fileUrl))
                return BadRequest(new { message = "File URL is required" });

            if (expiryMinutes < 1 || expiryMinutes > 1440) // Max 24 hours
                return BadRequest(new { message = "Expiry minutes must be between 1 and 1440" });

            var downloadUrl = await _fileStorageService.GetDownloadUrlAsync(fileUrl, expiryMinutes);

            return Ok(new
            {
                downloadUrl,
                expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating download URL for: {FileUrl}", fileUrl);
            return StatusCode(500, new { message = "Error generating download URL", error = ex.Message });
        }
    }

    #region Private Helper Methods

    private async Task<IActionResult> UploadFile(IFormFile file, string folder, string category)
    {
        var result = await UploadFileInternal(file, folder, category);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new
        {
            url = result.Url,
            fileName = result.FileName,
            message = "File uploaded successfully"
        });
    }

    private async Task<FileUploadResult> UploadFileInternal(IFormFile file, string folder, string category)
    {
        try
        {
            // Validate file
            if (file == null || file.Length == 0)
                return new FileUploadResult { Success = false, Message = "No file provided" };

            // Validate file size
            if (!MaxFileSizes.TryGetValue(category, out var maxSize))
                return new FileUploadResult { Success = false, Message = "Invalid category" };

            if (file.Length > maxSize)
            {
                var maxSizeMb = maxSize / (1024 * 1024);
                return new FileUploadResult
                {
                    Success = false,
                    Message = $"File size exceeds maximum allowed size of {maxSizeMb} MB"
                };
            }

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions[category].Contains(extension))
            {
                return new FileUploadResult
                {
                    Success = false,
                    Message = $"File type not allowed. Allowed types: {string.Join(", ", AllowedExtensions[category])}"
                };
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";

            // Upload to storage
            using var stream = file.OpenReadStream();
            var fileUrl = await _fileStorageService.UploadFileAsync(
                stream,
                fileName,
                file.ContentType,
                folder
            );

            _logger.LogInformation("File uploaded successfully: {FileName} to {Folder}", fileName, folder);

            return new FileUploadResult
            {
                Success = true,
                Url = fileUrl,
                FileName = fileName,
                Message = "File uploaded successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return new FileUploadResult
            {
                Success = false,
                Message = $"Error uploading file: {ex.Message}"
            };
        }
    }

    private class FileUploadResult
    {
        public bool Success { get; set; }
        public string? Url { get; set; }
        public string? FileName { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    #endregion
}
