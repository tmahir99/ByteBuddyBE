namespace JwtAuthAspNet7WebAPI.Core.Dtos
{
    namespace JwtAuthAspNet7WebAPI.Core.Dtos
    {
        public class FriendshipDto
        {
            public string RequesterId { get; set; }
            public string AddresseeId { get; set; }
            public string Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public ApplicationUserDto Requester { get; set; }
            public ApplicationUserDto Addressee { get; set; }
        }

        public class CommentDto
        {
            public long Id { get; set; }
            public string Content { get; set; }
            public DateTime CreatedAt { get; set; }
            public string CreatedById { get; set; }
            public ApplicationUserDto CreatedBy { get; set; }
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
            public string TaggedUserId { get; set; }
            public ApplicationUserDto TaggedUser { get; set; }
            public long CodeSnippetId { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
