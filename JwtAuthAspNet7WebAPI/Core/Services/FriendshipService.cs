using JwtAuthAspNet7WebAPI.Core.DbContext;
using JwtAuthAspNet7WebAPI.Core.Dtos;
using JwtAuthAspNet7WebAPI.Core.Entities;
using JwtAuthAspNet7WebAPI.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JwtAuthAspNet7WebAPI.Core.Services
{
    public class FriendshipService : IFriendshipService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FriendshipService> _logger;

        public FriendshipService(ApplicationDbContext context, ILogger<FriendshipService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Helper to resolve userId or username to userId
        private async Task<string> ResolveUserIdAsync(string userIdOrUserName)
        {
            if (string.IsNullOrWhiteSpace(userIdOrUserName)) return null;
            // Try as userId first
            var user = await _context.Users.FindAsync(userIdOrUserName);
            if (user != null) return user.Id;
            // Try as username (case-insensitive)
            user = await _context.Users.FirstOrDefaultAsync(u => u.UserName.ToLower() == userIdOrUserName.ToLower());
            return user?.Id;
        }

        public async Task<FriendshipDto> SendFriendRequestAsync(string requesterId, string addresseeId)
        {
            try
            {
                requesterId = await ResolveUserIdAsync(requesterId);
                addresseeId = await ResolveUserIdAsync(addresseeId);
                if (string.IsNullOrWhiteSpace(requesterId) || string.IsNullOrWhiteSpace(addresseeId))
                {
                    _logger.LogWarning("SendFriendRequest called with null or empty user IDs");
                    throw new ArgumentException("User IDs cannot be null or empty");
                }
                if (requesterId == addresseeId)
                {
                    _logger.LogWarning("User {UserId} attempted to send friend request to themselves", requesterId);
                    throw new InvalidOperationException("Cannot send friend request to yourself");
                }
                _logger.LogInformation("Sending friend request from {RequesterId} to {AddresseeId}", requesterId, addresseeId);
                var requester = await _context.Users.FindAsync(requesterId);
                var addressee = await _context.Users.FindAsync(addresseeId);
                if (requester == null || addressee == null)
                {
                    _logger.LogWarning("Friend request failed: One or both users not found - Requester: {RequesterId}, Addressee: {AddresseeId}", requesterId, addresseeId);
                    throw new KeyNotFoundException("One or both users not found");
                }
                var existingFriendship = await _context.Friendships
                    .FirstOrDefaultAsync(f =>
                        (f.RequesterId == requesterId && f.AddresseeId == addresseeId) ||
                        (f.RequesterId == addresseeId && f.AddresseeId == requesterId));
                if (existingFriendship != null)
                {
                    _logger.LogWarning("Friend request failed: Relationship already exists between {RequesterId} and {AddresseeId}", requesterId, addresseeId);
                    throw new InvalidOperationException("Friendship request already exists");
                }
                var friendship = new Friendship
                {
                    RequesterId = requesterId,
                    AddresseeId = addresseeId,
                    Status = FriendshipStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Friendships.Add(friendship);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Friend request sent successfully from {RequesterId} to {AddresseeId}", requesterId, addresseeId);
                return MapToDto(friendship);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending friend request from {RequesterId} to {AddresseeId}", requesterId, addresseeId);
                throw;
            }
        }

        public async Task<FriendshipDto> AcceptFriendRequestAsync(string requesterId, string addresseeId)
        {
            try
            {
                requesterId = await ResolveUserIdAsync(requesterId);
                addresseeId = await ResolveUserIdAsync(addresseeId);
                if (string.IsNullOrWhiteSpace(requesterId) || string.IsNullOrWhiteSpace(addresseeId))
                {
                    _logger.LogWarning("AcceptFriendRequest called with null or empty user IDs");
                    throw new ArgumentException("User IDs cannot be null or empty");
                }
                _logger.LogInformation("Accepting friend request from {RequesterId} to {AddresseeId}", requesterId, addresseeId);
                var friendship = await _context.Friendships
                    .Include(f => f.Requester)
                    .Include(f => f.Addressee)
                    .FirstOrDefaultAsync(f =>
                        f.RequesterId == requesterId &&
                        f.AddresseeId == addresseeId &&
                        f.Status == FriendshipStatus.Pending);
                if (friendship == null)
                {
                    _logger.LogWarning("Friend request not found for acceptance - Requester: {RequesterId}, Addressee: {AddresseeId}", requesterId, addresseeId);
                    throw new KeyNotFoundException("Friend request not found");
                }
                friendship.Status = FriendshipStatus.Accepted;
                friendship.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Friend request accepted successfully from {RequesterId} to {AddresseeId}", requesterId, addresseeId);
                return MapToDto(friendship);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting friend request from {RequesterId} to {AddresseeId}", requesterId, addresseeId);
                throw;
            }
        }

        public async Task DeclineFriendRequestAsync(string requesterId, string addresseeId)
        {
            try
            {
                requesterId = await ResolveUserIdAsync(requesterId);
                addresseeId = await ResolveUserIdAsync(addresseeId);
                if (string.IsNullOrWhiteSpace(requesterId) || string.IsNullOrWhiteSpace(addresseeId))
                {
                    _logger.LogWarning("DeclineFriendRequest called with null or empty user IDs");
                    throw new ArgumentException("User IDs cannot be null or empty");
                }
                _logger.LogInformation("Declining friend request from {RequesterId} to {AddresseeId}", requesterId, addresseeId);
                var friendship = await _context.Friendships
                    .FirstOrDefaultAsync(f =>
                        f.RequesterId == requesterId &&
                        f.AddresseeId == addresseeId &&
                        f.Status == FriendshipStatus.Pending);
                if (friendship == null)
                {
                    _logger.LogWarning("Friend request not found for decline - Requester: {RequesterId}, Addressee: {AddresseeId}", requesterId, addresseeId);
                    throw new KeyNotFoundException("Friend request not found");
                }
                friendship.Status = FriendshipStatus.Declined;
                friendship.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Friend request declined successfully from {RequesterId} to {AddresseeId}", requesterId, addresseeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error declining friend request from {RequesterId} to {AddresseeId}", requesterId, addresseeId);
                throw;
            }
        }

        public async Task BlockUserAsync(string userId, string blockedUserId)
        {
            try
            {
                userId = await ResolveUserIdAsync(userId);
                blockedUserId = await ResolveUserIdAsync(blockedUserId);
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(blockedUserId))
                {
                    _logger.LogWarning("BlockUser called with null or empty user IDs");
                    throw new ArgumentException("User IDs cannot be null or empty");
                }
                if (userId == blockedUserId)
                {
                    _logger.LogWarning("User {UserId} attempted to block themselves", userId);
                    throw new InvalidOperationException("Cannot block yourself");
                }
                _logger.LogInformation("Blocking user {BlockedUserId} by {UserId}", blockedUserId, userId);
                var existingFriendship = await _context.Friendships
                    .FirstOrDefaultAsync(f =>
                        (f.RequesterId == userId && f.AddresseeId == blockedUserId) ||
                        (f.RequesterId == blockedUserId && f.AddresseeId == userId));
                if (existingFriendship != null)
                {
                    _context.Friendships.Remove(existingFriendship);
                    _logger.LogInformation("Removed existing friendship between {UserId} and {BlockedUserId}", userId, blockedUserId);
                }
                var blockFriendship = new Friendship
                {
                    RequesterId = userId,
                    AddresseeId = blockedUserId,
                    Status = FriendshipStatus.Blocked,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Friendships.Add(blockFriendship);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User {BlockedUserId} blocked successfully by {UserId}", blockedUserId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error blocking user {BlockedUserId} by {UserId}", blockedUserId, userId);
                throw;
            }
        }

        public async Task<List<FriendshipDto>> GetFriendRequestsAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("GetFriendRequests called with null or empty userId");
                    throw new ArgumentException("User ID cannot be null or empty");
                }

                _logger.LogInformation("Getting friend requests for user {UserId}", userId);

                var friendRequests = await _context.Friendships
                    .Include(f => f.Requester)
                    .Include(f => f.Addressee)
                    .Where(f =>
                        f.AddresseeId == userId &&
                        f.Status == FriendshipStatus.Pending)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} friend requests for user {UserId}", friendRequests.Count, userId);
                return friendRequests.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting friend requests for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<ApplicationUserDto>> GetFriendsAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("GetFriends called with null or empty userId");
                    throw new ArgumentException("User ID cannot be null or empty");
                }

                _logger.LogInformation("Getting friends for user {UserId}", userId);

                var friendships = await _context.Friendships
                    .Include(f => f.Requester)
                    .Include(f => f.Addressee)
                    .Where(f =>
                        (f.RequesterId == userId || f.AddresseeId == userId) &&
                        f.Status == FriendshipStatus.Accepted)
                    .ToListAsync();

                var friends = friendships.Select(f =>
                {
                    var friend = f.RequesterId == userId ? f.Addressee : f.Requester;
                    return new ApplicationUserDto
                    {
                        Id = friend.Id,
                        UserName = friend.UserName,
                        FirstName = friend.FirstName,
                        LastName = friend.LastName,
                        Email = friend.Email
                    };
                }).ToList();

                _logger.LogInformation("Found {Count} friends for user {UserId}", friends.Count, userId);
                return friends;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting friends for user {UserId}", userId);
                throw;
            }
        }

        public async Task<FriendshipDto> GetFriendshipStatusAsync(string userId, string otherUserIdOrUserName)
        {
            userId = await ResolveUserIdAsync(userId);
            var otherUserId = await ResolveUserIdAsync(otherUserIdOrUserName);
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(otherUserId))
                return null;
            var friendship = await _context.Friendships
                .Include(f => f.Requester)
                .Include(f => f.Addressee)
                .FirstOrDefaultAsync(f =>
                    (f.RequesterId == userId && f.AddresseeId == otherUserId) ||
                    (f.RequesterId == otherUserId && f.AddresseeId == userId));
            return friendship != null ? MapToDto(friendship) : null;
        }

        private static FriendshipDto MapToDto(Friendship friendship)
        {
            return new FriendshipDto
            {
                RequesterId = friendship.RequesterId,
                RequesterName = friendship.Requester?.UserName,
                AddresseeId = friendship.AddresseeId,
                AddresseeName = friendship.Addressee?.UserName,
                Status = friendship.Status,
                CreatedAt = friendship.CreatedAt
            };
        }
    }
}