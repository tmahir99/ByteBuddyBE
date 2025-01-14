namespace JwtAuthAspNet7WebAPI.Core.Entities
{
    public class Comment
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedById { get; set; }
        public ApplicationUser CreatedBy { get; set; }
        public long CodeSnippetId { get; set; }
        public CodeSnippet CodeSnippet { get; set; }
    }
}
