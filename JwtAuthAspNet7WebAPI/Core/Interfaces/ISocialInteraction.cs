using JwtAuthAspNet7WebAPI.Core.Dtos;

namespace JwtAuthAspNet7WebAPI.Core.Interfaces
{
    public interface ISocialInteractionService
    {
        Task<LikeDto> ToggleLikeAsync(string userId, long codeSnippetId);
        Task<CommentDto> AddCommentAsync(string userId, CreateCommentDto dto);
        Task DeleteCommentAsync(string userId, long commentId);
        Task<UserTagDto> TagUserAsync(string taggerId, string taggedUserId, long codeSnippetId);
        Task RemoveUserTagAsync(string taggerId, long tagId);
        Task<List<CommentDto>> GetCommentsForSnippetAsync(long snippetId);
        Task<List<UserTagDto>> GetUserTagsForSnippetAsync(long snippetId);
    }
}