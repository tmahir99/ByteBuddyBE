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
        [ProducesResponseType(typeof(FriendshipDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<FriendshipDto>> SendFriendRequest(string addresseeId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _friendshipService.SendFriendRequestAsync(userId, addresseeId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("accept-request/{requesterId}")]
        [ProducesResponseType(typeof(FriendshipDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FriendshipDto>> AcceptFriendRequest(string requesterId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _friendshipService.AcceptFriendRequestAsync(requesterId, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Friend request not found" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("decline-request/{requesterId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeclineFriendRequest(string requesterId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _friendshipService.DeclineFriendRequestAsync(requesterId, userId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Friend request not found" });
            }
        }

        [HttpGet("requests")]
        [ProducesResponseType(typeof(List<FriendshipDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<FriendshipDto>>> GetFriendRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var requests = await _friendshipService.GetFriendRequestsAsync(userId);
            return Ok(requests);
        }

        [HttpGet("friends")]
        [ProducesResponseType(typeof(List<ApplicationUserDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ApplicationUserDto>>> GetFriends()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var friends = await _friendshipService.GetFriendsAsync(userId);
            return Ok(friends);
        }

        [HttpGet("request-status/{otherUser}")]
        [ProducesResponseType(typeof(FriendshipDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FriendshipDto>> GetFriendshipStatus(string otherUser)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var status = await _friendshipService.GetFriendshipStatusAsync(userId, otherUser);
            if (status == null)
                return NotFound(new { status = "NotFound" });
            return Ok(status);
        }
    }
}