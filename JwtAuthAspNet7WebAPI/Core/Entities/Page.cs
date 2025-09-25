using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JwtAuthAspNet7WebAPI.Core.Entities
{
    public class Page
    {
        [Key]
        public long Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; }
        
        [StringLength(1000)]
        public string Description { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; }
        
        [Required]
        [StringLength(450)]
        public string CreatedById { get; set; }
        
        [ForeignKey("CreatedById")]
        public virtual ApplicationUser CreatedBy { get; set; }
        
        public long? FileId { get; set; } // Nullable: link to uploaded file
        [ForeignKey("FileId")]
        public virtual FileEntity File { get; set; } // Navigation property
        public string FileUrl => File?.FileUrl; // Convenience property
        
        // Navigation Properties
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}
