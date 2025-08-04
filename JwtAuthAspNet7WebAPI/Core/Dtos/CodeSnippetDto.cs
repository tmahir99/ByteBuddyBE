using JwtAuthAspNet7WebAPI.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace JwtAuthAspNet7WebAPI.Core.Dtos
{
    public class CodeSnippetDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string CodeContent { get; set; }
        public string Description { get; set; }
        public string ProgrammingLanguage { get; set; }
        public string? FileUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public ApplicationUserDto CreatedBy { get; set; }
        public ICollection<Tag> Tags { get; set; }
        public int LikesCount { get; set; }
        public List<string>? LikedByUsers { get; set; }
        public int CommentsCount { get; set; }
        public string CreatedById { get; set; }
    }
    
    public class CreateCodeSnippetDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; }
        
        [Required(ErrorMessage = "Code content is required")]
        public string CodeContent { get; set; }
        
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }
        
        [Required(ErrorMessage = "Programming language is required")]
        [StringLength(50, ErrorMessage = "Programming language cannot exceed 50 characters")]
        public string ProgrammingLanguage { get; set; }
        
        [Url(ErrorMessage = "File URL must be a valid URL")]
        public string? FileUrl { get; set; }
        
        public string CreatedById { get; set; }
        public List<long>? TagIds { get; set; }
    }

    public class CodeSnippetFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? ProgrammingLanguage { get; set; }
        public string? CreatedById { get; set; }
        public string? SortBy { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class UpdateCodeSnippetDto
    {
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string? Title { get; set; }
        
        public string? CodeContent { get; set; }
        
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }
        
        [StringLength(50, ErrorMessage = "Programming language cannot exceed 50 characters")]
        public string? ProgrammingLanguage { get; set; }
        
        [Url(ErrorMessage = "File URL must be a valid URL")]
        public string? FileUrl { get; set; }
        
        public string CreatedById { get; set; }
        public List<long>? TagIds { get; set; }
    }

}
