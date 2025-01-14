namespace JwtAuthAspNet7WebAPI.Core.Entities
{
    public class Like
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public long? CodeSnippetId { get; set; }
        public CodeSnippet? CodeSnippet { get; set; }
        public long? PageId { get; set; }
        public Page? Page { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}