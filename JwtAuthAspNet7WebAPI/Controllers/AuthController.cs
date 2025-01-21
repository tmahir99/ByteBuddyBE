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
            var seerRoles = await _authService.SeedRolesAsync();

            return Ok(seerRoles);
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
        //[Authorize(Roles = StaticUserRoles.ADMIN)]
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
        [Authorize(Roles =StaticUserRoles.ADMIN)]
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
    }
}
