namespace JwtAuthAspNet7WebAPI.Core.Entities
{
    /// <summary>
    /// Status of a friendship request or relationship.
    /// </summary>
    public enum FriendshipStatus
    {
        /// <summary>Request is pending.</summary>
        Pending,
        /// <summary>Request is accepted.</summary>
        Accepted,
        /// <summary>Request is declined.</summary>
        Declined,
        /// <summary>User is blocked.</summary>
        Blocked
    }
}
