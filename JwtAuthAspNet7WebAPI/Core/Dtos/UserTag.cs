namespace JwtAuthAspNet7WebAPI.Core.Entities
{
    public class UserTag
    {
        public long Id { get; set; }
        public string TaggedUserId { get; set; }
        public ApplicationUser TaggedUser { get; set; }
        public string TaggerId { get; set; }
        public ApplicationUser Tagger { get; set; }
        public long CodeSnippetId { get; set; }
        public CodeSnippet CodeSnippet { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}