namespace JwtAuthAspNet7WebAPI.Core.Entities
{
    public class Page
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedById { get; set; }
        public ApplicationUser CreatedBy { get; set; }
        public ICollection<Like> Likes { get; set; }
    }
}
