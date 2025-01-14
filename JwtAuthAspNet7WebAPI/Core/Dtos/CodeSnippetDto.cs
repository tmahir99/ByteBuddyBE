using JwtAuthAspNet7WebAPI.Core.Entities;

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
        public int CommentsCount { get; set; }
        public string CreatedById { get; set; }
    }
    public class CreateCodeSnippetDto
    {
        public string Title { get; set; }
        public string CodeContent { get; set; }
        public string Description { get; set; }
        public string ProgrammingLanguage { get; set; }
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
        public string? Title { get; set; }
        public string? CodeContent { get; set; }
        public string? Description { get; set; }
        public string? ProgrammingLanguage { get; set; }
        public string? FileUrl { get; set; }
        public string CreatedById { get; set; }
        public List<long>? TagIds { get; set; }
    }

}
