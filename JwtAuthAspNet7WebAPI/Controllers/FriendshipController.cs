using JwtAuthAspNet7WebAPI.Core.Dtos.JwtAuthAspNet7WebAPI.Core.Dtos;
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
    public class FriendshipsController : ControllerBase
    {
        private readonly IFriendshipService _friendshipService;

        public FriendshipsController(IFriendshipService friendshipService)
        {
            _friendshipService = friendshipService;
        }

        [HttpPost("send-request/{addresseeId}")]
        public async Task<ActionResult<FriendshipDto>> SendFriendRequest(string addresseeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _friendshipService.SendFriendRequestAsync(userId, addresseeId);
            return Ok(result);
        }

        [HttpPost("accept-request/{requesterId}")]
        public async Task<ActionResult<FriendshipDto>> AcceptFriendRequest(string requesterId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _friendshipService.AcceptFriendRequestAsync(requesterId, userId);
            return Ok(result);
        }

        [HttpPost("decline-request/{requesterId}")]
        public async Task<IActionResult> DeclineFriendRequest(string requesterId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _friendshipService.DeclineFriendRequestAsync(requesterId, userId);
            return NoContent();
        }

        [HttpGet("requests")]
        public async Task<ActionResult<List<FriendshipDto>>> GetFriendRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var requests = await _friendshipService.GetFriendRequestsAsync(userId);
            return Ok(requests);
        }

        [HttpGet("friends")]
        public async Task<ActionResult<List<ApplicationUserDto>>> GetFriends()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var friends = await _friendshipService.GetFriendsAsync(userId);
            return Ok(friends);
        }
    }
}