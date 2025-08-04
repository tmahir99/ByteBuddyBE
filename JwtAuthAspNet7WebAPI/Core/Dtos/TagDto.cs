using System.ComponentModel.DataAnnotations;

namespace JwtAuthAspNet7WebAPI.Core.Dtos
{
    public class TagDto
    {
        public long Id { get; set; }
        
        [Required(ErrorMessage = "Tag name is required")]
        [StringLength(50, ErrorMessage = "Tag name cannot exceed 50 characters")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Tag area is required")]
        [StringLength(100, ErrorMessage = "Tag area cannot exceed 100 characters")]
        public string Area { get; set; }
    }
}
