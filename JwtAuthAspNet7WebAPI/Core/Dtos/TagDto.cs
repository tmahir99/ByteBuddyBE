using System.ComponentModel.DataAnnotations;

namespace JwtAuthAspNet7WebAPI.Core.Dtos
{
    public class TagDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Area { get; set; }
    }
}
