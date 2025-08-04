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
        [ProducesResponseType(typeof(LikeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LikeDto>> ToggleLike(long snippetId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _socialService.ToggleLikeAsync(userId, snippetId);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = $"Code snippet with ID {snippetId} not found" });
            }
        }

        [HttpPost("snippets/{snippetId}/comments")]
        [ProducesResponseType(typeof(CommentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommentDto>> AddComment(long snippetId, [FromBody] CreateCommentDto dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                dto.CodeSnippetId = snippetId;
                var result = await _socialService.AddCommentAsync(userId, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = $"Code snippet with ID {snippetId} not found" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("comments/{commentId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteComment(long commentId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _socialService.DeleteCommentAsync(userId, commentId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = $"Comment with ID {commentId} not found" });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost("snippets/{snippetId}/tags/{taggedUserId}")]
        [ProducesResponseType(typeof(UserTagDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserTagDto>> TagUser(long snippetId, string taggedUserId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _socialService.TagUserAsync(userId, taggedUserId, snippetId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("tags/{tagId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RemoveUserTag(long tagId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _socialService.RemoveUserTagAsync(userId, tagId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = $"User tag with ID {tagId} not found" });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet("snippets/{snippetId}/comments")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<CommentDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CommentDto>>> GetComments(long snippetId)
        {
            var comments = await _socialService.GetCommentsForSnippetAsync(snippetId);
            return Ok(comments);
        }

        [HttpGet("snippets/{snippetId}/tags")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<UserTagDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<UserTagDto>>> GetUserTags(long snippetId)
        {
            var tags = await _socialService.GetUserTagsForSnippetAsync(snippetId);
            return Ok(tags);
        }
    }
}