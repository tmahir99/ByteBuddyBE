using JwtAuthAspNet7WebAPI.Core.Dtos.JwtAuthAspNet7WebAPI.Core.Dtos;
using JwtAuthAspNet7WebAPI.Core.Dtos;
using JwtAuthAspNet7WebAPI.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JwtAuthAspNet7WebAPI.Controllers
{

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SocialInteractionsController : ControllerBase
    {
        private readonly ISocialInteractionService _socialService;

        public SocialInteractionsController(ISocialInteractionService socialService)
        {
            _socialService = socialService;
        }

        [HttpPost("snippets/{snippetId}/like")]
        public async Task<ActionResult<LikeDto>> ToggleLike(long snippetId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _socialService.ToggleLikeAsync(userId, snippetId);
            return Ok(result);
        }

        [HttpPost("snippets/{snippetId}/comments")]
        public async Task<ActionResult<CommentDto>> AddComment(long snippetId, [FromBody] CreateCommentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            dto.CodeSnippetId = snippetId;
            var result = await _socialService.AddCommentAsync(userId, dto);
            return Ok(result);
        }

        [HttpDelete("comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(long commentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _socialService.DeleteCommentAsync(userId, commentId);
            return NoContent();
        }

        [HttpPost("snippets/{snippetId}/tags/{taggedUserId}")]
        public async Task<ActionResult<UserTagDto>> TagUser(long snippetId, string taggedUserId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _socialService.TagUserAsync(userId, taggedUserId, snippetId);
            return Ok(result);
        }

        [HttpDelete("tags/{tagId}")]
        public async Task<IActionResult> RemoveUserTag(long tagId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _socialService.RemoveUserTagAsync(userId, tagId);
            return NoContent();
        }

        [HttpGet("snippets/{snippetId}/comments")]
        [AllowAnonymous]
        public async Task<ActionResult<List<CommentDto>>> GetComments(long snippetId)
        {
            var comments = await _socialService.GetCommentsForSnippetAsync(snippetId);
            return Ok(comments);
        }

        [HttpGet("snippets/{snippetId}/tags")]
        [AllowAnonymous]
        public async Task<ActionResult<List<UserTagDto>>> GetUserTags(long snippetId)
        {
            var tags = await _socialService.GetUserTagsForSnippetAsync(snippetId);
            return Ok(tags);
        }
    }
}