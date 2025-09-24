using JwtAuthAspNet7WebAPI.Core.Dtos;

namespace JwtAuthAspNet7WebAPI.Core.Interfaces
{
    public interface IFriendshipService
    {
        Task<FriendshipDto> SendFriendRequestAsync(string requesterId, string addresseeId);
        Task<FriendshipDto> AcceptFriendRequestAsync(string requesterId, string addresseeId);
        Task DeclineFriendRequestAsync(string requesterId, string addresseeId);
        Task BlockUserAsync(string userId, string blockedUserId);
        Task<List<FriendshipDto>> GetFriendRequestsAsync(string userId);
        Task<List<ApplicationUserDto>> GetFriendsAsync(string userId);
    }

}