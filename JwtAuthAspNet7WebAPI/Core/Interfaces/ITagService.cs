using JwtAuthAspNet7WebAPI.Core.Dtos;
using JwtAuthAspNet7WebAPI.Core.Entities;

namespace JwtAuthAspNet7WebAPI.Core.Interfaces
{
    public interface ITagService
    {
        Task<TagDto> CreateAsync(TagDto dto);
        Task<TagDto> GetByIdAsync(long id);
        Task<List<string>> GetAllTagNames();
        Task<List<TagDto>> GetAllTagsAsync();
        Task<List<string>> GetAllTagAreas();
        Task UpdateAsync(long id, TagDto dto);
        Task DeleteAsync(long id);
    }
}
