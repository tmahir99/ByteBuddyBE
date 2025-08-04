using System.ComponentModel.DataAnnotations;

namespace JwtAuthAspNet7WebAPI.Core.Dtos
{
    public class FileUploadDto
    {
        [Required(ErrorMessage = "File is required")]
        public IFormFile File { get; set; }
        
        [Required(ErrorMessage = "Code snippet ID is required")]
        public long CodeSnippetId { get; set; }
    }

    public class FileUploadResponseDto
    {
        public bool IsSucceed { get; set; }
        public string Message { get; set; }
        public string FileUrl { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; }
    }

    public class FileDeleteDto
    {
        [Required(ErrorMessage = "Code snippet ID is required")]
        public long CodeSnippetId { get; set; }
    }
}
