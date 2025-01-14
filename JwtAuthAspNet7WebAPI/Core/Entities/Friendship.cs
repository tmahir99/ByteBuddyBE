namespace JwtAuthAspNet7WebAPI.Core.Entities
{
    public class Friendship
    {
        public string RequesterId { get; set; }
        public ApplicationUser Requester { get; set; }
        public string AddresseeId { get; set; }
        public ApplicationUser Addressee { get; set; }
        public FriendshipStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
