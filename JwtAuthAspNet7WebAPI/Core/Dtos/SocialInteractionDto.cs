using JwtAuthAspNet7WebAPI.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace JwtAuthAspNet7WebAPI.Core.Dtos
{
    public class FriendshipDto
    {
        public string RequesterId { get; set; }
        public string RequesterName { get; set; }
        public string AddresseeId { get; set; }
        public string AddresseeName { get; set; }
        public FriendshipStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CommentDto
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public ApplicationUserDto CreatedBy { get; set; }
        public long CodeSnippetId { get; set; }
    }

    public class CreateCommentDto
    {
        [Required(ErrorMessage = "Content is required")]
        [StringLength(1000, ErrorMessage = "Content cannot exceed 1000 characters")]
        public string Content { get; set; }
        
        public long CodeSnippetId { get; set; }
    }

    public class LikeDto
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUserDto User { get; set; }
        public long? CodeSnippetId { get; set; }
        public long? PageId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserTagDto
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUserDto User { get; set; }
        public long CodeSnippetId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
