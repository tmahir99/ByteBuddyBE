using JwtAuthAspNet7WebAPI.Core.DbContext;
using JwtAuthAspNet7WebAPI.Core.Dtos;
using JwtAuthAspNet7WebAPI.Core.Dtos.JwtAuthAspNet7WebAPI.Core.Dtos;
using JwtAuthAspNet7WebAPI.Core.Entities;
using JwtAuthAspNet7WebAPI.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthAspNet7WebAPI.Core.Services
{
    public class FriendshipService : IFriendshipService
    {
        private readonly ApplicationDbContext _context;

        public FriendshipService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<FriendshipDto> SendFriendRequestAsync(string requesterId, string addresseeId)
        {
            // Validate users exist
            var requester = await _context.Users.FindAsync(requesterId);
            var addressee = await _context.Users.FindAsync(addresseeId);

            if (requester == null || addressee == null)
            {
                throw new KeyNotFoundException("One or both users not found");
            }

            // Check if friendship already exists
            var existingFriendship = await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.RequesterId == requesterId && f.AddresseeId == addresseeId) ||
                    (f.RequesterId == addresseeId && f.AddresseeId == requesterId));

            if (existingFriendship != null)
            {
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

            return MapToDto(friendship);
        }

        public async Task<FriendshipDto> AcceptFriendRequestAsync(string requesterId, string addresseeId)
        {
            var friendship = await _context.Friendships
                .Include(f => f.Requester)
                .Include(f => f.Addressee)
                .FirstOrDefaultAsync(f =>
                    f.RequesterId == requesterId &&
                    f.AddresseeId == addresseeId &&
                    f.Status == FriendshipStatus.Pending);

            if (friendship == null)
            {
                throw new KeyNotFoundException("Friend request not found");
            }

            friendship.Status = FriendshipStatus.Accepted;
            await _context.SaveChangesAsync();

            return MapToDto(friendship);
        }

        public async Task DeclineFriendRequestAsync(string requesterId, string addresseeId)
        {
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    f.RequesterId == requesterId &&
                    f.AddresseeId == addresseeId &&
                    f.Status == FriendshipStatus.Pending);

            if (friendship == null)
            {
                throw new KeyNotFoundException("Friend request not found");
            }

            friendship.Status = FriendshipStatus.Declined;
            await _context.SaveChangesAsync();
        }

        public async Task BlockUserAsync(string userId, string blockedUserId)
        {
            // Remove any existing friendship
            var existingFriendship = await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.RequesterId == userId && f.AddresseeId == blockedUserId) ||
                    (f.RequesterId == blockedUserId && f.AddresseeId == userId));

            if (existingFriendship != null)
            {
                _context.Friendships.Remove(existingFriendship);
            }

            // Create blocked relationship
            var blockFriendship = new Friendship
            {
                RequesterId = userId,
                AddresseeId = blockedUserId,
                Status = FriendshipStatus.Blocked,
                CreatedAt = DateTime.UtcNow
            };

            _context.Friendships.Add(blockFriendship);
            await _context.SaveChangesAsync();
        }

        public async Task<List<FriendshipDto>> GetFriendRequestsAsync(string userId)
        {
            var friendRequests = await _context.Friendships
                .Include(f => f.Requester)
                .Include(f => f.Addressee)
                .Where(f =>
                    f.AddresseeId == userId &&
                    f.Status == FriendshipStatus.Pending)
                .ToListAsync();

            return friendRequests.Select(MapToDto).ToList();
        }

        public async Task<List<ApplicationUserDto>> GetFriendsAsync(string userId)
        {
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
                    UserName = friend.UserName,
                    FirstName = friend.FirstName,
                    LastName = friend.LastName,
                    Email = friend.Email
                };
            }).ToList();

            return friends;
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