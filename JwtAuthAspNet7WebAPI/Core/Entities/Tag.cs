using System.ComponentModel.DataAnnotations;

namespace JwtAuthAspNet7WebAPI.Core.Entities
{
    public class Tag
    {
        [Key]
        public long Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Area { get; set; }
        
        // Navigation Properties
        public virtual ICollection<CodeSnippet> CodeSnippets { get; set; } = new List<CodeSnippet>();
    }
}
