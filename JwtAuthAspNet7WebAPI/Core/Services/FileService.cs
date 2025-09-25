using JwtAuthAspNet7WebAPI.Core.Dtos;
using JwtAuthAspNet7WebAPI.Core.Entities;
using JwtAuthAspNet7WebAPI.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using JwtAuthAspNet7WebAPI.Core.DbContext;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace JwtAuthAspNet7WebAPI.Core.Services
{
    public class FileService : IFileService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileService> _logger;
        private readonly IWebHostEnvironment _environment;

        // Allowed file types
        private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        private readonly string[] _allowedFileExtensions = { ".txt", ".md", ".json", ".xml", ".csv", ".log", ".sql", ".js", ".ts", ".html", ".css", ".py", ".cs", ".java", ".cpp", ".c", ".h", ".php", ".rb", ".go", ".rs", ".kt", ".swift", ".yml", ".yaml" };
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB
        private readonly long _maxImageSize = 5 * 1024 * 1024; // 5MB

        public FileService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            ILogger<FileService> logger,
            IWebHostEnvironment environment)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public async Task<FileUploadResponseDto> UploadFileAsync(IFormFile file, long codeSnippetId, string userId)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Upload attempted with null or empty file");
                    return new FileUploadResponseDto
                    {
                        IsSucceed = false,
                        Message = "No file provided"
                    };
                }

                _logger.LogInformation("Starting file upload for CodeSnippet {CodeSnippetId} by user {UserId}", codeSnippetId, userId);

                // Validate file
                var validationResult = await ValidateFileAsync(file);
                if (!validationResult)
                {
                    return new FileUploadResponseDto
                    {
                        IsSucceed = false,
                        Message = "File validation failed"
                    };
                }

                // Check if code snippet exists and user owns it
                var codeSnippet = await _context.CodeSnippets
                    .FirstOrDefaultAsync(cs => cs.Id == codeSnippetId && cs.CreatedById == userId);

                if (codeSnippet == null)
                {
                    _logger.LogWarning("CodeSnippet {CodeSnippetId} not found or user {UserId} doesn't own it", codeSnippetId, userId);
                    return new FileUploadResponseDto
                    {
                        IsSucceed = false,
                        Message = "Code snippet not found or you don't have permission to modify it"
                    };
                }

                // Delete existing file if present
                if (!string.IsNullOrEmpty(codeSnippet.FileUrl))
                {
                    await DeleteExistingFileAsync(codeSnippet.FileUrl);
                }

                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "codesnippets");
                
                // Ensure directory exists
                Directory.CreateDirectory(uploadsFolder);
                
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Process image if it's an image file
                FileUploadResponseDto processResult = null;
                if (IsImageFile(file.FileName))
                {
                    processResult = await ProcessImageAsync(file, filePath);
                    if (!processResult.IsSucceed)
                    {
                        // Clean up uploaded file if image processing failed
                        if (File.Exists(filePath))
                            File.Delete(filePath);
                        return processResult;
                    }
                }

                // Generate file URL
                var fileUrl = $"/uploads/codesnippets/{uniqueFileName}";

                // Update code snippet with file URL
                codeSnippet.FileUrl = fileUrl;
                await _context.SaveChangesAsync();

                _logger.LogInformation("File uploaded successfully for CodeSnippet {CodeSnippetId}: {FileName}", codeSnippetId, uniqueFileName);

                return new FileUploadResponseDto
                {
                    IsSucceed = true,
                    Message = "File uploaded successfully",
                    FileUrl = fileUrl,
                    FileName = uniqueFileName,
                    FileSize = file.Length,
                    ContentType = file.ContentType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file for CodeSnippet {CodeSnippetId}", codeSnippetId);
                return new FileUploadResponseDto
                {
                    IsSucceed = false,
                    Message = "An error occurred while uploading the file"
                };
            }
        }

        public async Task<FileUploadResponseDto> UploadFileAsync(IFormFile file, string userId)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Upload attempted with null or empty file");
                    return new FileUploadResponseDto
                    {
                        IsSucceed = false,
                        Message = "No file provided"
                    };
                }

                _logger.LogInformation("Starting independent file upload by user {UserId}", userId);

                var validationResult = await ValidateFileAsync(file);
                if (!validationResult)
                {
                    return new FileUploadResponseDto
                    {
                        IsSucceed = false,
                        Message = "File validation failed"
                    };
                }

                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "files");
                Directory.CreateDirectory(uploadsFolder);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                FileUploadResponseDto processResult = null;
                if (IsImageFile(file.FileName))
                {
                    processResult = await ProcessImageAsync(file, filePath);
                    if (!processResult.IsSucceed)
                    {
                        if (File.Exists(filePath))
                            File.Delete(filePath);
                        return processResult;
                    }
                }

                var fileUrl = $"/uploads/files/{uniqueFileName}";

                // Save file entity to DB
                var fileEntity = new FileEntity
                {
                    FileName = uniqueFileName,
                    FileUrl = fileUrl,
                    FileSize = file.Length,
                    ContentType = file.ContentType,
                    UploadedById = userId
                };
                _context.Files.Add(fileEntity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("File uploaded successfully: {FileName}", uniqueFileName);

                return new FileUploadResponseDto
                {
                    FileId = fileEntity.FileId,
                    IsSucceed = true,
                    Message = "File uploaded successfully",
                    FileUrl = fileUrl,
                    FileName = uniqueFileName,
                    FileSize = file.Length,
                    ContentType = file.ContentType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return new FileUploadResponseDto
                {
                    IsSucceed = false,
                    Message = "An error occurred while uploading the file"
                };
            }
        }

        public async Task<FileUploadResponseDto> DeleteFileAsync(long codeSnippetId, string userId)
        {
            try
            {
                _logger.LogInformation("Deleting file for CodeSnippet {CodeSnippetId} by user {UserId}", codeSnippetId, userId);

                var codeSnippet = await _context.CodeSnippets
                    .FirstOrDefaultAsync(cs => cs.Id == codeSnippetId && cs.CreatedById == userId);

                if (codeSnippet == null)
                {
                    _logger.LogWarning("CodeSnippet {CodeSnippetId} not found or user {UserId} doesn't own it", codeSnippetId, userId);
                    return new FileUploadResponseDto
                    {
                        IsSucceed = false,
                        Message = "Code snippet not found or you don't have permission to modify it"
                    };
                }

                if (string.IsNullOrEmpty(codeSnippet.FileUrl))
                {
                    return new FileUploadResponseDto
                    {
                        IsSucceed = false,
                        Message = "No file attached to this code snippet"
                    };
                }

                // Delete physical file
                await DeleteExistingFileAsync(codeSnippet.FileUrl);

                // Update database
                codeSnippet.FileUrl = null;
                await _context.SaveChangesAsync();

                _logger.LogInformation("File deleted successfully for CodeSnippet {CodeSnippetId}", codeSnippetId);

                return new FileUploadResponseDto
                {
                    IsSucceed = true,
                    Message = "File deleted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file for CodeSnippet {CodeSnippetId}", codeSnippetId);
                return new FileUploadResponseDto
                {
                    IsSucceed = false,
                    Message = "An error occurred while deleting the file"
                };
            }
        }

        public async Task<bool> ValidateFileAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("File validation failed: null or empty file");
                    return false;
                }

                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var isImage = IsImageFile(file.FileName);
                var isAllowedFile = _allowedFileExtensions.Contains(fileExtension);

                if (!isImage && !isAllowedFile)
                {
                    _logger.LogWarning("File validation failed: unsupported file type {Extension}", fileExtension);
                    return false;
                }

                var maxSize = isImage ? _maxImageSize : _maxFileSize;
                if (file.Length > maxSize)
                {
                    _logger.LogWarning("File validation failed: file size {Size} exceeds limit {MaxSize}", file.Length, maxSize);
                    return false;
                }

                // Additional validation for images (cross-platform)
                if (isImage)
                {
                    try
                    {
                        using var stream = file.OpenReadStream();
                        using var image = await Image.LoadAsync<Rgba32>(stream);
                        if (image.Width > 4096 || image.Height > 4096)
                        {
                            _logger.LogWarning("Image validation failed: dimensions {Width}x{Height} exceed limit", image.Width, image.Height);
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Image validation failed: invalid image format");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during file validation");
                return false;
            }
        }

        public async Task<string> GetFilePathAsync(string fileName)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "codesnippets");
            return Path.Combine(uploadsFolder, fileName);
        }

        public async Task<bool> FileExistsAsync(string filePath)
        {
            return File.Exists(filePath);
        }

        public string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".txt" => "text/plain",
                ".md" => "text/markdown",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".csv" => "text/csv",
                ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".ts" => "application/typescript",
                _ => "application/octet-stream"
            };
        }

        public bool IsImageFile(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return _allowedImageExtensions.Contains(extension);
        }

        public async Task<FileUploadResponseDto> ProcessImageAsync(IFormFile file, string filePath)
        {
            try
            {
                _logger.LogInformation("Processing image: {FileName}", file.FileName);

                // Cross-platform image validation
                using var stream = file.OpenReadStream();
                using var image = await Image.LoadAsync<Rgba32>(stream);
                if (image == null)
                {
                    return new FileUploadResponseDto
                    {
                        IsSucceed = false,
                        Message = "Invalid or corrupted image format"
                    };
                }
                // Optionally: resize, optimize, etc.
                _logger.LogInformation("Image processed successfully: {FileName}", file.FileName);
                return new FileUploadResponseDto
                {
                    IsSucceed = true,
                    Message = "Image processed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing image: {FileName}", file.FileName);
                return new FileUploadResponseDto
                {
                    IsSucceed = false,
                    Message = "Failed to process image"
                };
            }
        }

        private async Task DeleteExistingFileAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                    return;

                var fileName = Path.GetFileName(fileUrl);
                var filePath = await GetFilePathAsync(fileName);

                if (await FileExistsAsync(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Deleted existing file: {FilePath}", filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting existing file: {FileUrl}", fileUrl);
                // Don't throw - this shouldn't prevent new file upload
            }
        }

        public async Task<FileEntity> GetFileEntityByIdAsync(long fileId)
        {
            return await _context.Files.FirstOrDefaultAsync(f => f.FileId == fileId);
        }
    }
}
