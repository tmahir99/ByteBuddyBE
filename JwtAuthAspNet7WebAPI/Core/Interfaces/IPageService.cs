using JwtAuthAspNet7WebAPI.Core.Dtos;
using JwtAuthAspNet7WebAPI.Core.Entities;

namespace JwtAuthAspNet7WebAPI.Core.Interfaces
{
    public interface IPageService
    {
        Task<PageDto> CreateAsync(CreatePageDto dto);
        Task<PageDto> GetByIdAsync(long id, string currentUserId = null);
        Task<PaginatedResult<PageDto>> GetAllAsync(PageFilterDto filter, string currentUserId = null);
        Task UpdateAsync(long id, UpdatePageDto dto);
        Task DeleteAsync(long id, string userId);
        Task<List<PageDto>> GetUserPagesAsync(string userId, string currentUserId = null);
        Task<PageDto> ToggleLikeAsync(long pageId, string userId);
        Task<List<ApplicationUserDto>> GetPageLikersAsync(long pageId);
    }
}
