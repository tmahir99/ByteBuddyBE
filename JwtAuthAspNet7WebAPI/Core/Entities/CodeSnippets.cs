namespace JwtAuthAspNet7WebAPI.Core.Entities
{
    public class CodeSnippet
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string CodeContent { get; set; }
        public string Description { get; set; }
        public string ProgrammingLanguage { get; set; }
        public string? FileUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedById { get; set; }
        public ApplicationUser CreatedBy { get; set; }
        public ICollection<Tag> Tags { get; set; }
        public ICollection<Like> Likes { get; set; }
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
