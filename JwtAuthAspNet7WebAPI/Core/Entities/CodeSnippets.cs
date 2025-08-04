using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JwtAuthAspNet7WebAPI.Core.Entities
{
    public class CodeSnippet
    {
        [Key]
        public long Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; }
        
        [Required]
        [Column(TypeName = "text")]
        public string CodeContent { get; set; }
        
        [StringLength(1000)]
        public string Description { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ProgrammingLanguage { get; set; }
        
        [Url]
        [StringLength(500)]
        public string? FileUrl { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; }
        
        [Required]
        [StringLength(450)] // Standard length for Identity user IDs
        public string CreatedById { get; set; }
        
        [ForeignKey("CreatedById")]
        public virtual ApplicationUser CreatedBy { get; set; }
        
        // Navigation Properties
        public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<UserTag> UserTags { get; set; } = new List<UserTag>();
    }
}
