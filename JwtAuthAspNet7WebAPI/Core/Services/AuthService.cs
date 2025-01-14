using JwtAuthAspNet7WebAPI.Core.Dtos;
using JwtAuthAspNet7WebAPI.Core.Entities;
using JwtAuthAspNet7WebAPI.Core.Interfaces;
using JwtAuthAspNet7WebAPI.Core.OtherObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtAuthAspNet7WebAPI.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task<AuthServiceResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.UserName);

            if (user is null)
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Invalid Credentials"
                };

            var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!isPasswordCorrect)
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Invalid Credentials"
                };

            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim("JWTID", Guid.NewGuid().ToString()),
        new Claim("FirstName", user.FirstName),
        new Claim("LastName", user.LastName),
        new Claim("DateOfBirth", user.DateOfBirth.ToString("yyyy-MM-dd")),
        new Claim("Gender", user.Gender),
        new Claim("BirthPlace", user.BirthPlace),
        new Claim("Address", user.Address)
    };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = GenerateNewJsonWebToken(authClaims);

            return new AuthServiceResponseDto()
            {
                IsSucceed = true,
                Message = token,
                Roles = userRoles.ToList()
            };
        }

        public async Task<AuthServiceResponseDto> MakeAdminAsync(UpdatePermissionDto updatePermissionDto)
        {
            var user = await _userManager.FindByNameAsync(updatePermissionDto.UserName);

            if (user is null)
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Invalid User name!!!!!!!!"
                };

            await _userManager.AddToRoleAsync(user, StaticUserRoles.ADMIN);

            return new AuthServiceResponseDto()
            {
                IsSucceed = true,
                Message = "User " + user + " is now an ADMIN"
            };
        }

        public async Task<AuthServiceResponseDto> MakeUserAsync(UpdatePermissionDto updatePermissionDto)
        {
            var user = await _userManager.FindByNameAsync(updatePermissionDto.UserName);

            if (user is null)
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Invalid User name!!!!!!!!"
                };

            await _userManager.AddToRoleAsync(user, StaticUserRoles.USER);

            return new AuthServiceResponseDto()
            {
                IsSucceed = true,
                Message = "Guest " + user + " is now an User"
            };
        }

        public async Task<AuthServiceResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var isExistsUser = await _userManager.FindByNameAsync(registerDto.UserName);

            if (isExistsUser != null)
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "UserName Already Exists"
                };

            // Also check if email exists
            var isExistsEmail = await _userManager.FindByEmailAsync(registerDto.Email);
            if (isExistsEmail != null)
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Email Already Exists"
                };

            ApplicationUser newUser = new ApplicationUser()
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                UserName = registerDto.UserName,
                SecurityStamp = Guid.NewGuid().ToString(),
                DateOfBirth = registerDto.DateOfBirth,
                Gender = registerDto.Gender,
                BirthPlace = registerDto.BirthPlace,
                Address = registerDto.Address
            };

            var createUserResult = await _userManager.CreateAsync(newUser, registerDto.Password);

            if (!createUserResult.Succeeded)
            {
                var errorString = "User Creation Failed Because: ";
                foreach (var error in createUserResult.Errors)
                {
                    errorString += " # " + error.Description;
                }
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = errorString
                };
            }

            await _userManager.AddToRoleAsync(newUser, StaticUserRoles.GUEST);

            return new AuthServiceResponseDto()
            {
                IsSucceed = true,
                Message = "User Created Successfully"
            };
        }

        public async Task<AuthServiceResponseDto> SeedRolesAsync()
        {
            bool isAdminRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.ADMIN);
            bool isUserRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.USER);
            bool isGuestRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.GUEST);

            if (isAdminRoleExists && isUserRoleExists && isGuestRoleExists)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = true,
                    Message = "Roles Seeding is Already Done",
                    Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync()
                };
            }

            if (!isUserRoleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.USER));
            }
            if (!isAdminRoleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.ADMIN));
            }
            if (!isGuestRoleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.GUEST));
            }

            return new AuthServiceResponseDto()
            {
                IsSucceed = true,
                Message = "Role Seeding Done Successfully",
                Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync()
            };
        }

        private string GenerateNewJsonWebToken(List<Claim> claims)
        {
            var authSecret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var tokenObject = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(1),
                    claims: claims,
                    signingCredentials: new SigningCredentials(authSecret, SecurityAlgorithms.HmacSha256)
                );

            string token = new JwtSecurityTokenHandler().WriteToken(tokenObject);

            return token;
        }

        public async Task<List<ApplicationUserDto>> GetAllUsersAsync()
        {
            var users = _userManager.Users.ToList();
            var userDtos = new List<ApplicationUserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new ApplicationUserDto
                {
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }

            return userDtos;
        }

        public async Task<List<string>> GetAllUserNamesAsync()
        {
            var userNames = _userManager.Users.Select(user => user.UserName).ToList();
            return userNames;
        }

        public async Task<AuthServiceResponseDto> RemoveAdminAsync(UpdatePermissionDto updatePermissionDto)
        {
            var user = await _userManager.FindByNameAsync(updatePermissionDto.UserName);

            if (user is null)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Invalid User name " + user
                };
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, StaticUserRoles.ADMIN);

            if (!isAdmin)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "User " + user + " is not an admin"
                };
            }

            var removeAdminResult = await _userManager.RemoveFromRoleAsync(user, StaticUserRoles.ADMIN);

            if (!removeAdminResult.Succeeded)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Failed to remove admin role for " + user
                };
            }

            return new AuthServiceResponseDto()
            {
                IsSucceed = true,
                Message = "User " + user + "  is no longer an admin"
            };
        }

        public async Task<AuthServiceResponseDto> RemoveUserAsync(UpdatePermissionDto updatePermissionDto)
        {
            var user = await _userManager.FindByNameAsync(updatePermissionDto.UserName);

            if (user is null)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Invalid User name " + user
                };
            }

            var isUser = await _userManager.IsInRoleAsync(user, StaticUserRoles.USER);

            if (!isUser)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "User " + user + " is not a USER"
                };
            }

            var removeUserResult = await _userManager.RemoveFromRoleAsync(user, StaticUserRoles.USER);

            if (!removeUserResult.Succeeded)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Failed to remove USER role for " + user
                };
            }

            return new AuthServiceResponseDto()
            {
                IsSucceed = true,
                Message = "User " + user + "  is no longer a USER"
            };
        }

        public async Task<AuthServiceResponseDto> RemoveGuestAsync(UpdatePermissionDto updatePermissionDto)
        {
            var user = await _userManager.FindByNameAsync(updatePermissionDto.UserName);

            if (user is null)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Invalid User name " + user
                };
            }

            var isGuest = await _userManager.IsInRoleAsync(user, StaticUserRoles.GUEST);

            if (!isGuest)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "User " + user + " is not a GUEST"
                };
            }

            var removeGuestResult = await _userManager.RemoveFromRoleAsync(user, StaticUserRoles.GUEST);

            if (!removeGuestResult.Succeeded)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Failed to remove GUEST role for " + user
                };
            }

            return new AuthServiceResponseDto()
            {
                IsSucceed = true,
                Message = "User " + user + "  is no longer a GUEST"
            };
        }
    }
}
