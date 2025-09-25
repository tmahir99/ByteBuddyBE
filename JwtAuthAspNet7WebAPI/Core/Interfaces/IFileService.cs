using JwtAuthAspNet7WebAPI.Core.Dtos;
using JwtAuthAspNet7WebAPI.Core.Entities;

namespace JwtAuthAspNet7WebAPI.Core.Interfaces
{
    public interface IFileService
    {
        Task<FileUploadResponseDto> UploadFileAsync(IFormFile file, string userId); // New independent upload
        Task<FileUploadResponseDto> UploadFileAsync(IFormFile file, long codeSnippetId, string userId); // Legacy
        Task<FileUploadResponseDto> DeleteFileAsync(long codeSnippetId, string userId);
        Task<bool> ValidateFileAsync(IFormFile file);
        Task<string> GetFilePathAsync(string fileName);
        Task<bool> FileExistsAsync(string filePath);
        string GetContentType(string fileName);
        bool IsImageFile(string fileName);
        Task<FileUploadResponseDto> ProcessImageAsync(IFormFile file, string filePath);
        Task<FileEntity> GetFileEntityByIdAsync(long fileId);
    }
}
