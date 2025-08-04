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
        
        // Navigation Properties
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}
