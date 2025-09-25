using JwtAuthAspNet7WebAPI.Core.Dtos;
using JwtAuthAspNet7WebAPI.Core.Interfaces;
using JwtAuthAspNet7WebAPI.Core.OtherObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JwtAuthAspNet7WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly ILogger<FileController> _logger;
        private readonly IWebHostEnvironment _environment;

        public FileController(IFileService fileService, ILogger<FileController> logger, IWebHostEnvironment environment)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        /// <summary>
        /// Upload a file or image for a code snippet
        /// </summary>
        /// <param name="file">File to upload</param>
        /// <param name="codeSnippetId">Code snippet ID</param>
        /// <returns>Upload result</returns>
        [HttpPost]
        [Route("upload/snippet")]
        [ProducesResponseType(typeof(FileUploadResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] long codeSnippetId)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Upload attempted with null or empty file");
                    return BadRequest(new { message = "No file provided" });
                }

                if (codeSnippetId <= 0)
                {
                    _logger.LogWarning("Upload attempted with invalid code snippet ID: {CodeSnippetId}", codeSnippetId);
                    return BadRequest(new { message = "Invalid code snippet ID" });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Upload attempted without valid user ID");
                    return Unauthorized(new { message = "User not authenticated" });
                }

                _logger.LogInformation("File upload requested by user {UserId} for CodeSnippet {CodeSnippetId}", userId, codeSnippetId);

                var result = await _fileService.UploadFileAsync(file, codeSnippetId, userId);

                if (result.IsSucceed)
                {
                    _logger.LogInformation("File uploaded successfully: {FileName}", result.FileName);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("File upload failed: {Message}", result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during file upload for CodeSnippet {CodeSnippetId}", codeSnippetId);
                return StatusCode(500, new { message = "An error occurred during file upload", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete file attached to a code snippet
        /// </summary>
        /// <param name="codeSnippetId">Code snippet ID</param>
        /// <returns>Delete result</returns>
        [HttpDelete]
        [Route("delete/{codeSnippetId}")]
        [ProducesResponseType(typeof(FileUploadResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteFile(long codeSnippetId)
        {
            try
            {
                if (codeSnippetId <= 0)
                {
                    _logger.LogWarning("Delete attempted with invalid code snippet ID: {CodeSnippetId}", codeSnippetId);
                    return BadRequest(new { message = "Invalid code snippet ID" });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Delete attempted without valid user ID");
                    return Unauthorized(new { message = "User not authenticated" });
                }

                _logger.LogInformation("File delete requested by user {UserId} for CodeSnippet {CodeSnippetId}", userId, codeSnippetId);

                var result = await _fileService.DeleteFileAsync(codeSnippetId, userId);

                if (result.IsSucceed)
                {
                    _logger.LogInformation("File deleted successfully for CodeSnippet {CodeSnippetId}", codeSnippetId);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("File delete failed: {Message}", result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during file delete for CodeSnippet {CodeSnippetId}", codeSnippetId);
                return StatusCode(500, new { message = "An error occurred during file deletion", details = ex.Message });
            }
        }

        /// <summary>
        /// Get file information for a code snippet
        /// </summary>
        /// <param name="codeSnippetId">Code snippet ID</param>
        /// <returns>File information</returns>
        [HttpGet]
        [Route("info/{codeSnippetId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetFileInfo(long codeSnippetId)
        {
            try
            {
                if (codeSnippetId <= 0)
                {
                    return BadRequest(new { message = "Invalid code snippet ID" });
                }

                // This would need to be implemented in the service
                // For now, return a placeholder response
                return Ok(new { message = "File info endpoint - to be implemented" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file info for CodeSnippet {CodeSnippetId}", codeSnippetId);
                return StatusCode(500, new { message = "An error occurred while getting file information", details = ex.Message });
            }
        }

        /// <summary>
        /// Download file attached to a code snippet
        /// </summary>
        /// <param name="codeSnippetId">Code snippet ID</param>
        /// <returns>File download</returns>
        [HttpGet]
        [Route("download/{codeSnippetId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DownloadFile(long codeSnippetId)
        {
            try
            {
                if (codeSnippetId <= 0)
                {
                    return BadRequest(new { message = "Invalid code snippet ID" });
                }

                // This would need to be implemented in the service
                // For now, return a placeholder response
                return Ok(new { message = "File download endpoint - to be implemented" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file for CodeSnippet {CodeSnippetId}", codeSnippetId);
                return StatusCode(500, new { message = "An error occurred during file download", details = ex.Message });
            }
        }

        /// <summary>
        /// Upload a file or image independently (not tied to a code snippet)
        /// </summary>
        /// <param name="file">File to upload</param>
        /// <returns>Upload result</returns>
        [HttpPost]
        [Route("upload")]
        [ProducesResponseType(typeof(FileUploadResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UploadFileIndependent([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Upload attempted with null or empty file");
                    return BadRequest(new { message = "No file provided" });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Upload attempted without valid user ID");
                    return Unauthorized(new { message = "User not authenticated" });
                }

                _logger.LogInformation("File upload requested by user {UserId}", userId);

                var result = await _fileService.UploadFileAsync(file, userId);

                if (result.IsSucceed)
                {
                    _logger.LogInformation("File uploaded successfully: {FileName}", result.FileName);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("File upload failed: {Message}", result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during file upload");
                return StatusCode(500, new { message = "An error occurred during file upload", details = ex.Message });
            }
        }

        /// <summary>
        /// Get/download a file or image by its unique fileId
        /// </summary>
        /// <param name="fileId">File ID</param>
        /// <returns>File binary stream</returns>
        [HttpGet("{fileId}")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetFileById(long fileId)
        {
            try
            {
                var fileEntity = await _fileService.GetFileEntityByIdAsync(fileId);
                if (fileEntity == null)
                    return NotFound(new { message = "File not found" });

                var filePath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "files", fileEntity.FileName);
                if (!System.IO.File.Exists(filePath))
                    return NotFound(new { message = "File not found on disk" });

                var contentType = fileEntity.ContentType ?? "application/octet-stream";
                var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                return File(stream, contentType, fileEntity.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file by id {FileId}", fileId);
                return StatusCode(500, new { message = "An error occurred while retrieving the file", details = ex.Message });
            }
        }
    }
}
