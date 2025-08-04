using JwtAuthAspNet7WebAPI.Core.Dtos;

namespace JwtAuthAspNet7WebAPI.Core.Interfaces
{
    public interface IFileService
    {
        Task<FileUploadResponseDto> UploadFileAsync(IFormFile file, long codeSnippetId, string userId);
        Task<FileUploadResponseDto> DeleteFileAsync(long codeSnippetId, string userId);
        Task<bool> ValidateFileAsync(IFormFile file);
        Task<string> GetFilePathAsync(string fileName);
        Task<bool> FileExistsAsync(string filePath);
        string GetContentType(string fileName);
        bool IsImageFile(string fileName);
        Task<FileUploadResponseDto> ProcessImageAsync(IFormFile file, string filePath);
    }
}
