namespace JwtAuthAspNet7WebAPI.Core.Entities
{
    public class UserTag
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public long CodeSnippetId { get; set; }
        public CodeSnippet CodeSnippet { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedById { get; set; }
        public ApplicationUser CreatedBy { get; set; }
    }
}