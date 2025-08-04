using JwtAuthAspNet7WebAPI.Core.DbContext;
using JwtAuthAspNet7WebAPI.Core.Dtos;
using JwtAuthAspNet7WebAPI.Core.Entities;
using JwtAuthAspNet7WebAPI.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JwtAuthAspNet7WebAPI.Core.Services
{
    public class SocialInteractionService : ISocialInteractionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SocialInteractionService> _logger;

        public SocialInteractionService(ApplicationDbContext context, ILogger<SocialInteractionService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<LikeDto> ToggleLikeAsync(string userId, long codeSnippetId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("ToggleLikeAsync called with null or empty userId");
                    throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
                }

                if (codeSnippetId <= 0)
                {
                    _logger.LogWarning("ToggleLikeAsync called with invalid codeSnippetId: {CodeSnippetId}", codeSnippetId);
                    throw new ArgumentException("Code snippet ID must be greater than 0", nameof(codeSnippetId));
                }

                _logger.LogInformation("Toggling like for user {UserId} on code snippet {CodeSnippetId}", userId, codeSnippetId);

                var existingLike = await _context.Likes
                    .FirstOrDefaultAsync(l => l.UserId == userId && l.CodeSnippetId == codeSnippetId);

                if (existingLike != null)
                {
                    // Unlike
                    _context.Likes.Remove(existingLike);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Like removed for user {UserId} on code snippet {CodeSnippetId}", userId, codeSnippetId);
                    return null;
                }
                else
                {
                    // Like
                    var like = new Like
                    {
                        UserId = userId,
                        CodeSnippetId = codeSnippetId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Likes.Add(like);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Like added for user {UserId} on code snippet {CodeSnippetId}", userId, codeSnippetId);

                    return await MapLikeToDtoAsync(like);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling like for user {UserId} on code snippet {CodeSnippetId}", userId, codeSnippetId);
                throw;
            }
        }

        public async Task<CommentDto> AddCommentAsync(string userId, CreateCommentDto dto)
        {
            // Validate that the code snippet exists
            var codeSnippet = await _context.CodeSnippets
                .FirstOrDefaultAsync(cs => cs.Id == dto.CodeSnippetId);

            if (codeSnippet == null)
            {
                throw new KeyNotFoundException($"Code snippet with ID {dto.CodeSnippetId} not found");
            }

            var comment = new Comment
            {
                Content = dto.Content.Trim(),
                CreatedById = userId,
                CodeSnippetId = dto.CodeSnippetId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return await MapCommentToDtoAsync(comment);
        }

        public async Task DeleteCommentAsync(string userId, long commentId)
        {
            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                throw new KeyNotFoundException($"Comment with ID {commentId} not found");
            }

            if (comment.CreatedById != userId)
            {
                throw new UnauthorizedAccessException("You can only delete your own comments");
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<UserTagDto> TagUserAsync(string taggerId, string taggedUserId, long codeSnippetId)
        {
            // Validate that both user and code snippet exist
            var taggedUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == taggedUserId);

            var codeSnippet = await _context.CodeSnippets
                .FirstOrDefaultAsync(cs => cs.Id == codeSnippetId);

            if (taggedUser == null)
            {
                throw new KeyNotFoundException($"User with ID {taggedUserId} not found");
            }

            if (codeSnippet == null)
            {
                throw new KeyNotFoundException($"Code snippet with ID {codeSnippetId} not found");
            }

            var userTag = new UserTag
            {
                UserId = taggedUserId,
                CodeSnippetId = codeSnippetId,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserTags.Add(userTag);
            await _context.SaveChangesAsync();

            return await MapUserTagToDtoAsync(userTag);
        }

        public async Task RemoveUserTagAsync(string taggerId, long tagId)
        {
            var userTag = await _context.UserTags
                .FirstOrDefaultAsync(ut => ut.Id == tagId);

            if (userTag == null)
            {
                throw new KeyNotFoundException($"User tag with ID {tagId} not found");
            }

            // Ensure only the original tagger can remove the tag
            var codeSnippet = await _context.CodeSnippets
                .FirstOrDefaultAsync(cs => cs.Id == userTag.CodeSnippetId);

            if (codeSnippet == null || codeSnippet.CreatedById != taggerId)
            {
                throw new UnauthorizedAccessException("You can only remove tags from your own code snippets");
            }

            _context.UserTags.Remove(userTag);
            await _context.SaveChangesAsync();
        }

        public async Task<List<CommentDto>> GetCommentsForSnippetAsync(long snippetId)
        {
            var comments = await _context.Comments
                .Include(c => c.CreatedBy)
                .Where(c => c.CodeSnippetId == snippetId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var commentDtos = new List<CommentDto>();
            foreach (var comment in comments)
            {
                commentDtos.Add(await MapCommentToDtoAsync(comment)); 
            }

            return commentDtos;
        }


        public async Task<List<UserTagDto>> GetUserTagsForSnippetAsync(long snippetId)
        {
            var userTags = await _context.UserTags
                .Include(ut => ut.User)
                .Where(ut => ut.CodeSnippetId == snippetId)
                .OrderByDescending(ut => ut.CreatedAt)
                .ToListAsync();

            var userTagDtos = await Task.WhenAll(userTags.Select(MapUserTagToDtoAsync));
            return userTagDtos.ToList();
        }

        private async Task<LikeDto> MapLikeToDtoAsync(Like like)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == like.UserId);

            return new LikeDto
            {
                Id = like.Id,
                UserId = like.UserId,
                User = new ApplicationUserDto
                {
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email
                },
                CodeSnippetId = like.CodeSnippetId,
                CreatedAt = like.CreatedAt
            };
        }

        private async Task<CommentDto> MapCommentToDtoAsync(Comment comment)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == comment.CreatedById);

            return new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                CreatedBy = new ApplicationUserDto
                {
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email
                },
                CodeSnippetId = comment.CodeSnippetId
            };
        }

        private async Task<UserTagDto> MapUserTagToDtoAsync(UserTag userTag)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userTag.UserId);

            return new UserTagDto
            {
                Id = userTag.Id,
                UserId = userTag.UserId,
                User = new ApplicationUserDto
                {
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email
                },
                CodeSnippetId = userTag.CodeSnippetId,
                CreatedAt = userTag.CreatedAt
            };
        }
    }
}