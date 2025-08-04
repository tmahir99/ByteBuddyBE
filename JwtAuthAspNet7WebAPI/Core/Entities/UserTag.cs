using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JwtAuthAspNet7WebAPI.Core.Entities
{
    public class UserTag
    {
        [Key]
        public long Id { get; set; }
        
        [Required]
        [StringLength(450)]
        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        
        [Required]
        public long CodeSnippetId { get; set; }
        
        [ForeignKey("CodeSnippetId")]
        public virtual CodeSnippet CodeSnippet { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; }
        
        [Required]
        [StringLength(450)]
        public string CreatedById { get; set; }
        
        [ForeignKey("CreatedById")]
        public virtual ApplicationUser CreatedBy { get; set; }
    }
}