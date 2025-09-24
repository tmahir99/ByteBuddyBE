using JwtAuthAspNet7WebAPI.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace JwtAuthAspNet7WebAPI.Core.Dtos
{
    /// <summary>
    /// Data transfer object for a friendship relationship between two users.
    /// </summary>
    public class FriendshipDto
    {
        /// <summary>
        /// The user ID of the requester.
        /// </summary>
        public string RequesterId { get; set; }
        /// <summary>
        /// The user name of the requester.
        /// </summary>
        public string RequesterName { get; set; }
        /// <summary>
        /// The user ID of the addressee.
        /// </summary>
        public string AddresseeId { get; set; }
        /// <summary>
        /// The user name of the addressee.
        /// </summary>
        public string AddresseeName { get; set; }
        /// <summary>
        /// The status of the friendship (Pending, Accepted, Declined, Blocked).
        /// </summary>
        public FriendshipStatus Status { get; set; }
        /// <summary>
        /// The date and time when the friendship was created.
        /// </summary>
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
