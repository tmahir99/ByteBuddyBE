using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JwtAuthAspNet7WebAPI.Core.Entities
{
    public class Like
    {
        [Key]
        public long Id { get; set; }
        
        [Required]
        [StringLength(450)]
        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        
        public long? CodeSnippetId { get; set; }
        
        [ForeignKey("CodeSnippetId")]
        public virtual CodeSnippet? CodeSnippet { get; set; }
        
        public long? PageId { get; set; }
        
        [ForeignKey("PageId")]
        public virtual Page? Page { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; }
    }
}