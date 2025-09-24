using JwtAuthAspNet7WebAPI.Core.Dtos;
using JwtAuthAspNet7WebAPI.Core.Interfaces;
using JwtAuthAspNet7WebAPI.Core.OtherObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthAspNet7WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // Route For Seeding my roles to DB
        [HttpPost]
        [Route("seed-roles")]
        public async Task<IActionResult> SeedRoles()
        {
            var seedRoles = await _authService.SeedRolesAsync();
            return Ok(seedRoles);
        }


        // Route -> Register
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var registerResult = await _authService.RegisterAsync(registerDto);

            if (registerResult.IsSucceed)
                return Ok(registerResult);

            return BadRequest(registerResult);
        }


        // Route -> Login
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var loginResult = await _authService.LoginAsync(loginDto);

            if (loginResult.IsSucceed)
                return Ok(loginResult);

            return Unauthorized(loginResult);
        }


        // Route -> make guest -> admin
        [HttpPost]
        [Route("make-admin")]
        [Authorize(Roles = StaticUserRoles.ADMIN)]
        public async Task<IActionResult> MakeAdmin([FromBody] UpdatePermissionDto updatePermissionDto)
        {
            var operationResult = await _authService.MakeAdminAsync(updatePermissionDto);

            if (operationResult.IsSucceed)
                return Ok(operationResult);

            return BadRequest(operationResult);
        }

        // Route -> make guest -> user
        [HttpPost]
        [Route("make-user")]
        [Authorize(Roles = StaticUserRoles.ADMIN)]
        public async Task<IActionResult> MakeUser([FromBody] UpdatePermissionDto updatePermissionDto)
        {
            var operationResult = await _authService.MakeUserAsync(updatePermissionDto);

            if (operationResult.IsSucceed)
                return Ok(operationResult);

            return BadRequest(operationResult);
        }

        [HttpGet]
        [Route("GetAllUsers")]
        [Authorize(Roles = StaticUserRoles.ADMIN)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _authService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet]
        [Route("GetAllUserNames")]
        public async Task<IActionResult> GetAllUsersNames()
        {
            var users = await _authService.GetAllUserNamesAsync();
            return Ok(users);
        }

        [HttpPost]
        [Route("remove-admin")]
        [Authorize(Roles = StaticUserRoles.ADMIN)]
        public async Task<IActionResult> RemoveAdmin([FromBody] UpdatePermissionDto updatePermissionDto)
        {
            var users = await _authService.RemoveAdminAsync(updatePermissionDto);
            return Ok(users);
        }

        [HttpPost]
        [Route("remove-user")]
        [Authorize(Roles = StaticUserRoles.ADMIN)]
        public async Task<IActionResult> RemoveUser([FromBody] UpdatePermissionDto updatePermissionDto)
        {
            var users = await _authService.RemoveUserAsync(updatePermissionDto);
            return Ok(users);
        }

        /// <summary>
        /// Activate user email with activation token
        /// </summary>
        /// <param name="token">Email activation token</param>
        /// <returns>Activation result</returns>
        [HttpGet]
        [Route("activate")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthServiceResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ActivateEmail([FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return BadRequest(new { message = "Activation token is required" });
                }

                var result = await _authService.ActivateEmailAsync(token);

                if (result.IsSucceed)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during email activation", details = ex.Message });
            }
        }

        /// <summary>
        /// Resend activation email to user
        /// </summary>
        /// <param name="resendDto">Resend activation email data</param>
        /// <returns>Operation result</returns>
        [HttpPost]
        [Route("resend-activation")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthServiceResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ResendActivationEmail([FromBody] ResendActivationDto resendDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _authService.ResendActivationEmailAsync(resendDto.Email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while resending activation email", details = ex.Message });
            }
        }

        /// <summary>
        /// Get all users with full information (public endpoint)
        /// </summary>
        /// <returns>List of all users with full info</returns>
        [HttpGet("all-public")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<ApplicationUserDto>), 200)]
        public async Task<IActionResult> GetAllUsersPublic()
        {
            var users = await _authService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Get a user's full information by username or id (public endpoint)
        /// </summary>
        /// <param name="user">Username or user id</param>
        /// <returns>User info</returns>
        [HttpGet("public-user")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApplicationUserDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetPublicUser([FromQuery] string user)
        {
            if (string.IsNullOrWhiteSpace(user))
                return BadRequest(new { message = "Username or user id is required" });

            var users = await _authService.GetAllUsersAsync();
            var foundUser = users.FirstOrDefault(u => u.UserName == user || u.Id == user);
            if (foundUser == null)
                return NotFound(new { message = "User not found" });
            return Ok(foundUser);
        }
    }
}
