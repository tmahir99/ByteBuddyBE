namespace JwtAuthAspNet7WebAPI.Core.Entities
{
    public class Tag
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Area { get; set; }
        public ICollection<CodeSnippet> CodeSnippets { get; set; }
    }
}
