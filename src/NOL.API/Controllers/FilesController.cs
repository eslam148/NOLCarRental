using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using NOL.Application.Common.Responses;
using NOL.Domain.Enums;

namespace NOL.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all file operations
public class FilesController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FilesController> _logger;
    private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".txt" };

    public FilesController(IWebHostEnvironment environment, ILogger<FilesController> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Upload a single file
    /// </summary>
    /// <param name="file">File to upload</param>
    /// <param name="folder">Optional folder name (e.g., "cars", "advertisements", "users")</param>
    /// <returns>File upload result with URL</returns>
    [HttpPost("upload")]
    public async Task<ActionResult<ApiResponse<FileUploadResultDto>>> UploadFile(
        IFormFile file, 
        [FromQuery] string? folder = null)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponse<FileUploadResultDto>
                {
                    Succeeded = false,
                    Message = "No file provided",
                    StatusCode = ApiStatusCode.BadRequest
                });
            }

            // Validate file size
            if (file.Length > _maxFileSize)
            {
                return BadRequest(new ApiResponse<FileUploadResultDto>
                {
                    Succeeded = false,
                    Message = $"File size exceeds maximum allowed size of {_maxFileSize / (1024 * 1024)}MB",
                    StatusCode = ApiStatusCode.BadRequest
                });
            }

            // Validate file extension
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new ApiResponse<FileUploadResultDto>
                {
                    Succeeded = false,
                    Message = $"File type not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}",
                    StatusCode = ApiStatusCode.BadRequest
                });
            }

            // Create upload directory
            var uploadFolder = string.IsNullOrEmpty(folder) ? "uploads" : $"uploads/{folder}";
            var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadPath = Path.Combine(webRootPath, uploadFolder);
            
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Generate file URL
            var fileUrl = $"/{uploadFolder}/{fileName}";
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.LogInformation("File uploaded successfully: {FileName} by user {UserId}", fileName, userId);

            var result = new FileUploadResultDto
            {
                FileName = fileName,
                OriginalFileName = file.FileName,
                FileSize = file.Length,
                FileUrl = fileUrl,
                UploadedAt = DateTime.UtcNow,
                UploadedBy = userId
            };

            return Ok(new ApiResponse<FileUploadResultDto>
            {
                Succeeded = true,
                Message = "File uploaded successfully",
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", file?.FileName);
            return StatusCode(500, new ApiResponse<FileUploadResultDto>
            {
                Succeeded = false,
                Message = "An error occurred while uploading the file",
                StatusCode = ApiStatusCode.InternalServerError
            });
        }
    }

    /// <summary>
    /// Upload multiple files
    /// </summary>
    /// <param name="files">Files to upload</param>
    /// <param name="folder">Optional folder name</param>
    /// <returns>List of file upload results</returns>
    [HttpPost("upload-multiple")]
    public async Task<ActionResult<ApiResponse<List<FileUploadResultDto>>>> UploadMultipleFiles(
        List<IFormFile> files, 
        [FromQuery] string? folder = null)
    {
        try
        {
            if (files == null || !files.Any())
            {
                return BadRequest(new ApiResponse<List<FileUploadResultDto>>
                {
                    Succeeded = false,
                    Message = "No files provided",
                    StatusCode = ApiStatusCode.BadRequest
                });
            }

            var results = new List<FileUploadResultDto>();
            var errors = new List<string>();

            foreach (var file in files)
            {
                try
                {
                    // Validate file size
                    if (file.Length > _maxFileSize)
                    {
                        errors.Add($"File {file.FileName} exceeds maximum size");
                        continue;
                    }

                    // Validate file extension
                    var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!_allowedExtensions.Contains(fileExtension))
                    {
                        errors.Add($"File {file.FileName} has unsupported type");
                        continue;
                    }

                    // Create upload directory
                    var uploadFolder = string.IsNullOrEmpty(folder) ? "uploads" : $"uploads/{folder}";
                    var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var uploadPath = Path.Combine(webRootPath, uploadFolder);
                    
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    // Generate unique filename
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var filePath = Path.Combine(uploadPath, fileName);

                    // Save file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Generate file URL
                    var fileUrl = $"/{uploadFolder}/{fileName}";
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    results.Add(new FileUploadResultDto
                    {
                        FileName = fileName,
                        OriginalFileName = file.FileName,
                        FileSize = file.Length,
                        FileUrl = fileUrl,
                        UploadedAt = DateTime.UtcNow,
                        UploadedBy = userId
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
                    errors.Add($"Error uploading {file.FileName}: {ex.Message}");
                }
            }

            var response = new ApiResponse<List<FileUploadResultDto>>
            {
                Succeeded = results.Any(),
                Message = results.Any() ? "Files uploaded successfully" : "No files were uploaded",
                Data = results,
                Errors = errors,
                StatusCode = results.Any() ? ApiStatusCode.Success : ApiStatusCode.BadRequest
            };

            return results.Any() ? Ok(response) : BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading multiple files");
            return StatusCode(500, new ApiResponse<List<FileUploadResultDto>>
            {
                Succeeded = false,
                Message = "An error occurred while uploading files",
                StatusCode = ApiStatusCode.InternalServerError
            });
        }
    }

    /// <summary>
    /// Delete a file
    /// </summary>
    /// <param name="fileName">Name of the file to delete</param>
    /// <param name="folder">Optional folder name</param>
    /// <returns>Deletion result</returns>
    [HttpDelete("{fileName}")]
    public async Task<ActionResult<ApiResponse>> DeleteFile(
        string fileName, 
        [FromQuery] string? folder = null)
    {
        try
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest(new ApiResponse
                {
                    Succeeded = false,
                    Message = "File name is required",
                    StatusCode = ApiStatusCode.BadRequest
                });
            }

            var uploadFolder = string.IsNullOrEmpty(folder) ? "uploads" : $"uploads/{folder}";
            var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var filePath = Path.Combine(webRootPath, uploadFolder, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new ApiResponse
                {
                    Succeeded = false,
                    Message = "File not found",
                    StatusCode = ApiStatusCode.NotFound
                });
            }

            System.IO.File.Delete(filePath);
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.LogInformation("File deleted successfully: {FileName} by user {UserId}", fileName, userId);

            return Ok(new ApiResponse
            {
                Succeeded = true,
                Message = "File deleted successfully",
                StatusCode = ApiStatusCode.Success
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileName}", fileName);
            return StatusCode(500, new ApiResponse
            {
                Succeeded = false,
                Message = "An error occurred while deleting the file",
                StatusCode = ApiStatusCode.InternalServerError
            });
        }
    }

    /// <summary>
    /// Get file information
    /// </summary>
    /// <param name="fileName">Name of the file</param>
    /// <param name="folder">Optional folder name</param>
    /// <returns>File information</returns>
    [HttpGet("{fileName}")]
    public ActionResult<ApiResponse<FileInfoDto>> GetFileInfo(
        string fileName, 
        [FromQuery] string? folder = null)
    {
        try
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest(new ApiResponse<FileInfoDto>
                {
                    Succeeded = false,
                    Message = "File name is required",
                    StatusCode = ApiStatusCode.BadRequest
                });
            }

            var uploadFolder = string.IsNullOrEmpty(folder) ? "uploads" : $"uploads/{folder}";
            var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var filePath = Path.Combine(webRootPath, uploadFolder, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new ApiResponse<FileInfoDto>
                {
                    Succeeded = false,
                    Message = "File not found",
                    StatusCode = ApiStatusCode.NotFound
                });
            }

            var fileInfo = new System.IO.FileInfo(filePath);
            var fileUrl = $"/{uploadFolder}/{fileName}";

            var result = new FileInfoDto
            {
                FileName = fileName,
                FileSize = fileInfo.Length,
                FileUrl = fileUrl,
                CreatedAt = fileInfo.CreationTimeUtc,
                ModifiedAt = fileInfo.LastWriteTimeUtc,
                Extension = fileInfo.Extension
            };

            return Ok(new ApiResponse<FileInfoDto>
            {
                Succeeded = true,
                Message = "File information retrieved successfully",
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file info: {FileName}", fileName);
            return StatusCode(500, new ApiResponse<FileInfoDto>
            {
                Succeeded = false,
                Message = "An error occurred while retrieving file information",
                StatusCode = ApiStatusCode.InternalServerError
            });
        }
    }
}

// DTOs
public class FileUploadResultDto
{
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string? UploadedBy { get; set; }
}

public class FileInfoDto
{
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string Extension { get; set; } = string.Empty;
}
